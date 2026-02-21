using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

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

public class NetworkManager : MonoBehaviour
{
    private Socket _socket;
    private byte[] _recvBuffer = new byte[1024];

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

        PacketHeader header = new PacketHeader { size = pktSize, id = 1000 };

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
                IntPtr ptr = Marshal.AllocHGlobal(bytesRead);
                Marshal.Copy(_recvBuffer, 0, ptr, bytesRead);

                PacketHeader header = (PacketHeader)Marshal.PtrToStructure(ptr, typeof(PacketHeader));

                if (header.id == 1001)
                {
                    int headerSize = Marshal.SizeOf(typeof(PacketHeader));
                    IntPtr dataPtr = new IntPtr(ptr.ToInt64() + headerSize);

                    S_LOGIN sLoginPkt = (S_LOGIN)Marshal.PtrToStructure(dataPtr, typeof(S_LOGIN));
                    Debug.Log($"Login Success. Player ID: {sLoginPkt.playerId}");
                }

                Marshal.FreeHGlobal(ptr);
                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceive, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Receive Error: {e.Message}");
        }
    }
}