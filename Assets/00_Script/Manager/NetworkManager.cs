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
    PKT_S_LEAVE_GAME = 1008,
    PKT_C_STANCE = 1009,
    PKT_S_STANCE = 1010,
    PKT_C_JUMP = 1011,
    PKT_S_JUMP = 1012,
    PKT_C_ATTACK = 1013,   
    PKT_S_ATTACK = 1014,   
}

enum ROOM : ushort
{
    ROOM_1,
    ROOM_2,
    ROOM_3,
}

public enum AttackType : ushort
{
    SLASH1 = 0,
    SLASH2,
    SLASH3,
    SKILL
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
    public int isRunning;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_MOVE
{
    public int playerId; 
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
    public int isRunning;
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

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_LEAVE_GAME
{
    public int playerId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_STANCE
{
    public int isStance;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_STANCE
{
    public int playerId;
    public int isStance;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_JUMP
{
    public int dummy;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_JUMP
{
    public int playerId;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_ATTACK
{
    public AttackType attackType;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_ATTACK
{
    public int playerId;
    public AttackType attackType;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public GameObject playerPrefab;
    public Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();
    public int myPlayerId;

    private Socket _socket;
    private byte[] _recvBuffer = new byte[1024];
    private List<byte> _packetBuffer = new List<byte>();

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

    public void SendMovePacket(Vector3 pos, float rotationY, bool isRunning)
    {
        if (_socket == null || !_socket.Connected) return;

        C_MOVE pkt = new C_MOVE { posX = pos.x, posY = pos.y, posZ = pos.z, rotY = rotationY, isRunning = isRunning ? 1 : 0 };
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

    public void SendStancePacket(bool isStance)
    {
        if (_socket == null || !_socket.Connected) return;

        C_STANCE pkt = new C_STANCE { isStance = isStance ? 1 : 0 };
        ushort pktSize = (ushort)(Marshal.SizeOf(typeof(PacketHeader)) + Marshal.SizeOf(typeof(C_STANCE)));
        PacketHeader header = new PacketHeader { size = pktSize, id = (ushort)PacketID.PKT_C_STANCE };

        byte[] sendBuffer = new byte[pktSize];
        IntPtr ptr = Marshal.AllocHGlobal(pktSize);

        Marshal.StructureToPtr(header, ptr, false);
        Marshal.StructureToPtr(pkt, ptr + Marshal.SizeOf(typeof(PacketHeader)), false);
        Marshal.Copy(ptr, sendBuffer, 0, pktSize);
        Marshal.FreeHGlobal(ptr);

        _socket.Send(sendBuffer);
    }

    public void SendJumpPacket()
    {
        if (_socket == null || !_socket.Connected) return;

        C_JUMP pkt = new C_JUMP { dummy = 1 };
        ushort pktSize = (ushort)(Marshal.SizeOf(typeof(PacketHeader)) + Marshal.SizeOf(typeof(C_JUMP)));
        PacketHeader header = new PacketHeader { size = pktSize, id = (ushort)PacketID.PKT_C_JUMP };

        byte[] sendBuffer = new byte[pktSize];
        IntPtr ptr = Marshal.AllocHGlobal(pktSize);

        Marshal.StructureToPtr(header, ptr, false);
        Marshal.StructureToPtr(pkt, ptr + Marshal.SizeOf(typeof(PacketHeader)), false);
        Marshal.Copy(ptr, sendBuffer, 0, pktSize);
        Marshal.FreeHGlobal(ptr);

        _socket.Send(sendBuffer);
    }

    public void SendAttackPacket(AttackType type)
    {
        if (_socket == null || !_socket.Connected) return;

        C_ATTACK pkt = new C_ATTACK { attackType = type };
        ushort pktSize = (ushort)(Marshal.SizeOf(typeof(PacketHeader)) + Marshal.SizeOf(typeof(C_ATTACK)));
        PacketHeader header = new PacketHeader { size = pktSize, id = (ushort)PacketID.PKT_C_ATTACK };

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
        if (_socket == null || !_socket.Connected) return;

        try
        {
            int bytesRead = _socket.EndReceive(ar);
            if (bytesRead > 0)
            {
                byte[] newBytes = new byte[bytesRead];
                Array.Copy(_recvBuffer, 0, newBytes, 0, bytesRead);
                _packetBuffer.AddRange(newBytes);

                while (true)
                {
                    int headerSize = Marshal.SizeOf(typeof(PacketHeader));

                    if (_packetBuffer.Count < headerSize) break;

                    byte[] headerBytes = _packetBuffer.GetRange(0, headerSize).ToArray();
                    IntPtr headerPtr = Marshal.AllocHGlobal(headerSize);
                    Marshal.Copy(headerBytes, 0, headerPtr, headerSize);
                    PacketHeader header = (PacketHeader)Marshal.PtrToStructure(headerPtr, typeof(PacketHeader));
                    Marshal.FreeHGlobal(headerPtr);

                    if (_packetBuffer.Count < header.size) break;

                    byte[] packetData = _packetBuffer.GetRange(0, header.size).ToArray();
                    _packetBuffer.RemoveRange(0, header.size);

                    IntPtr dataPtr = Marshal.AllocHGlobal(header.size);
                    Marshal.Copy(packetData, 0, dataPtr, header.size);
                    IntPtr payloadPtr = new IntPtr(dataPtr.ToInt64() + headerSize);

                    Debug.Log($"[추적] 완전한 조립 패킷! ID: {header.id}, Size: {header.size}");

                    if (header.id == (ushort)PacketID.PKT_S_LOGIN)
                    {
                        S_LOGIN sLoginPkt = (S_LOGIN)Marshal.PtrToStructure(payloadPtr, typeof(S_LOGIN));
                        myPlayerId = sLoginPkt.playerId;
                        Debug.Log($"로그인 성공! 내 ID: {myPlayerId}");
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
                                if (movement != null) movement.isMine = isMyCharacter;

                                if (isMyCharacter)
                                {
                                    GameObject camObj = GameObject.Find("PlayerCamera");
                                    if (camObj != null)
                                    {
                                        var vcam = camObj.GetComponent<Unity.Cinemachine.CinemachineCamera>();
                                        if (vcam != null) vcam.Target.TrackingTarget = go.transform;
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
                                        movement.syncIsRunning = (movePkt.isRunning == 1);
                                    }
                                }
                            });
                        }
                    }
                    else if (header.id == (ushort)PacketID.PKT_S_LEAVE_GAME)
                    {
                        S_LEAVE_GAME leavePkt = (S_LEAVE_GAME)Marshal.PtrToStructure(payloadPtr, typeof(S_LEAVE_GAME));
                        lock (_lock)
                        {
                            _packetQueue.Enqueue(() =>
                            {
                                if (_players.TryGetValue(leavePkt.playerId, out GameObject targetObj))
                                {
                                    Destroy(targetObj);
                                    _players.Remove(leavePkt.playerId);
                                    Debug.Log($"[퇴장] 플레이어({leavePkt.playerId})가 게임을 종료했습니다.");
                                }
                            });
                        }
                    }
                    else if (header.id == (ushort)PacketID.PKT_S_STANCE)
                    {
                        S_STANCE stancePkt = (S_STANCE)Marshal.PtrToStructure(payloadPtr, typeof(S_STANCE));
                        lock (_lock)
                        {
                            _packetQueue.Enqueue(() =>
                            {
                                if (stancePkt.playerId == myPlayerId) return;
                                if (_players.TryGetValue(stancePkt.playerId, out GameObject targetObj))
                                {
                                    SamuraiMovement movement = targetObj.GetComponent<SamuraiMovement>();
                                    if (movement != null) movement.ApplyRemoteStance(stancePkt.isStance == 1);
                                }
                            });
                        }
                    }
                    else if (header.id == (ushort)PacketID.PKT_S_JUMP)
                    {
                        S_JUMP jumpPkt = (S_JUMP)Marshal.PtrToStructure(payloadPtr, typeof(S_JUMP));

                        lock (_lock)
                        {
                            _packetQueue.Enqueue(() =>
                            {
                                // 내 캐릭터면 패스 (이미 스페이스바 누를 때 뛰었음)
                                if (jumpPkt.playerId == myPlayerId) return;

                                if (_players.TryGetValue(jumpPkt.playerId, out GameObject targetObj))
                                {
                                    SamuraiMovement movement = targetObj.GetComponent<SamuraiMovement>();
                                    // 상대방 스크립트의 '원격 점프' 함수 실행!
                                    if (movement != null) movement.ApplyRemoteJump();
                                }
                            });
                        }
                    }
                    else if (header.id == (ushort)PacketID.PKT_S_ATTACK)
                    {
                        S_ATTACK attackPkt = (S_ATTACK)Marshal.PtrToStructure(payloadPtr, typeof(S_ATTACK));

                        lock (_lock)
                        {
                            _packetQueue.Enqueue(() =>
                            {
                                if (attackPkt.playerId == myPlayerId) return;

                                if (_players.TryGetValue(attackPkt.playerId, out GameObject targetObj))
                                {
                                    SamuraiCombat combat = targetObj.GetComponent<SamuraiCombat>();
                                    if (combat != null) combat.ApplyRemoteAttack(attackPkt.attackType);
                                }
                            });
                        }
                    }
                    Marshal.FreeHGlobal(dataPtr);
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

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    private void Disconnect()
    {
        if (_socket != null && _socket.Connected)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
                Debug.Log("서버와의 연결을 명시적으로 종료했습니다.");
            }
            catch (Exception e)
            {
                Debug.LogError($"소켓 종료 중 에러: {e.Message}");
            }
        }
    }
}