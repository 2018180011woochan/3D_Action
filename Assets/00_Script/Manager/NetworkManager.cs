using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

public enum PacketID : ushort
{
    PKT_C_LOGIN = 1000,
    PKT_S_LOGIN = 1001,
    PKT_C_ENTER_GAME = 1002,
    PKT_S_ENTER_GAME = 1003,
    PKT_C_CHAT = 1004,
    PKT_S_CHAT = 1005,
    PKT_C_MOVE = 1006,
    PKT_S_MOVE = 1007,
}

enum ROOM : ushort
{
    ROOM_1,
    ROOM_2,
    ROOM_3,
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketHeader
{
    public ushort size;
    public ushort id;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_LOGIN
{
    public ulong dummyId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_LOGIN
{
    public int playerId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_MOVE
{
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_MOVE
{
    public int playerId; 
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_ENTER_GAME
{
    public int playerId;
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public GameObject playerPrefab;
    public Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();
    public int myPlayerId;

    private Socket _socket;
    private byte[] _recvBuffer = new byte[1024];

    private Queue<Action> _packetQueue = new Queue<Action>();
    private object _lock = new object();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _socket.Connect("127.0.0.1", 7777);
            Debug.Log("Connected to Server.");

            SendLoginPacket();
            _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceive, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection Failed: {e.Message}");
        }
    }

    private void SendLoginPacket()
    {
        C_LOGIN pkt = new C_LOGIN { dummyId = 7777777 };
        ushort pktSize = (ushort)(Marshal.SizeOf(typeof(PacketHeader)) + Marshal.SizeOf(typeof(C_LOGIN)));

        PacketHeader header = new PacketHeader { size = pktSize, id = (ushort)PacketID.PKT_C_LOGIN };

        byte[] sendBuffer = new byte[pktSize];
        IntPtr ptr = Marshal.AllocHGlobal(pktSize);

        Marshal.StructureToPtr(header, ptr, false);
        Marshal.StructureToPtr(pkt, ptr + Marshal.SizeOf(typeof(PacketHeader)), false);
        Marshal.Copy(ptr, sendBuffer, 0, pktSize);
        Marshal.FreeHGlobal(ptr);

        _socket.Send(sendBuffer);
    }

    public void SendMovePacket(Vector3 pos, float rotationY)
    {
        if (_socket == null || !_socket.Connected) return;

        C_MOVE pkt = new C_MOVE { posX = pos.x, posY = pos.y, posZ = pos.z, rotY = rotationY };
        ushort pktSize = (ushort)(Marshal.SizeOf(typeof(PacketHeader)) + Marshal.SizeOf(typeof(C_MOVE)));

        PacketHeader header = new PacketHeader { size = pktSize, id = (ushort)PacketID.PKT_C_MOVE };

        byte[] sendBuffer = new byte[pktSize];
        IntPtr ptr = Marshal.AllocHGlobal(pktSize);

        Marshal.StructureToPtr(header, ptr, false);
        Marshal.StructureToPtr(pkt, ptr + Marshal.SizeOf(typeof(PacketHeader)), false);
        Marshal.Copy(ptr, sendBuffer, 0, pktSize);
        Marshal.FreeHGlobal(ptr);

        _socket.Send(sendBuffer);
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            int bytesRead = _socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                int offset = 0;

                while (offset < bytesRead)
                {
                    int headerSize = Marshal.SizeOf(typeof(PacketHeader));
                    IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
                    Marshal.Copy(_recvBuffer, offset, headerPtr, headerSize);
                    PacketHeader header = (PacketHeader)Marshal.PtrToStructure(headerPtr, typeof(PacketHeader));
                    Marshal.FreeHGlobal(headerPtr);

                    IntPtr dataPtr = Marshal.AllocHGlobal(header.size);
                    Marshal.Copy(_recvBuffer, offset, dataPtr, header.size);

                    IntPtr payloadPtr = new IntPtr(dataPtr.ToInt64() + headerSize);

                    if (header.id == (ushort)PacketID.PKT_S_LOGIN)
                    {
                        S_LOGIN sLoginPkt = (S_LOGIN)Marshal.PtrToStructure(payloadPtr, typeof(S_LOGIN));
                        myPlayerId = sLoginPkt.playerId;
                        Debug.Log($"Login Success. My ID: {myPlayerId}");
                    }
                    else if (header.id == (ushort)PacketID.PKT_S_ENTER_GAME)
                    {
                        S_ENTER_GAME spawnPkt = (S_ENTER_GAME)Marshal.PtrToStructure(payloadPtr, typeof(S_ENTER_GAME));

                        lock (_lock)
                        {
                            _packetQueue.Enqueue(() =>
                            {
                                if (_players.ContainsKey(spawnPkt.playerId)) return;

                                Vector3 spawnPos = new Vector3(spawnPkt.posX, spawnPkt.posY, spawnPkt.posZ);
                                Quaternion spawnRot = Quaternion.Euler(0, spawnPkt.rotY, 0);

                                GameObject go = Instantiate(playerPrefab, spawnPos, spawnRot);
                                _players.Add(spawnPkt.playerId, go);

                                bool isMyCharacter = (spawnPkt.playerId == myPlayerId);

                                SamuraiMovement movement = go.GetComponent<SamuraiMovement>();
                                if (movement != null)
                                {
                                    movement.isMine = isMyCharacter;
                                }

                                if (isMyCharacter)
                                {
                                    GameObject camObj = GameObject.Find("PlayerCamera");
                                    if (camObj != null)
                                    {
                                        var vcam = camObj.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                                        if (vcam != null)
                                        {
                                            vcam.Target.TrackingTarget = go.transform;
                                        }
                                    }
                                    Debug.Log($"[스폰] 내 캐릭터({spawnPkt.playerId}) 생성 완료 & 카메라 셋업 끝!");
                                }
                                else
                                {
                                    Debug.Log($"[스폰] 다른 유저({spawnPkt.playerId}) 입장 완료!");
                                }
                            });
                        }
                    }
                    else if (header.id == (ushort)PacketID.PKT_S_MOVE)
                    {
                        S_MOVE movePkt = (S_MOVE)Marshal.PtrToStructure(payloadPtr, typeof(S_MOVE));

                        lock (_lock)
                        {
                            _packetQueue.Enqueue(() =>
                            {
                                if (movePkt.playerId == myPlayerId) return;

                                if (_players.TryGetValue(movePkt.playerId, out GameObject targetObj))
                                {
                                    SamuraiMovement movement = targetObj.GetComponent<SamuraiMovement>();
                                    if (movement != null)
                                    {
                                        movement.syncPos = new Vector3(movePkt.posX, movePkt.posY, movePkt.posZ);
                                        movement.syncRotY = movePkt.rotY;
                                    }
                                }
                            });
                        }
                    }
                    Marshal.FreeHGlobal(dataPtr);
                    offset += header.size;
                }

                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceive, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Receive Error: {e.Message}");
        }
    }

    private void Update()
    {
        lock (_lock)
        {
            while (_packetQueue.Count > 0)
            {
                Action action = _packetQueue.Dequeue();
                action.Invoke();
            }
        }
    }
}