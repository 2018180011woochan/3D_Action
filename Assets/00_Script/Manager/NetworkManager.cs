using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

// 1. C++ 서버와 메모리 구조를 완벽하게 동일하게 맞추는 마법의 속성
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

public class NetworkManager : MonoBehaviour
{
    Socket _socket;

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        // 2. TCP 소켓 생성 및 서버 연결
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            _socket.Connect("127.0.0.1", 7777);
            Debug.Log("? 서버 연결 성공!");

            SendLoginPacket();
        }
        catch (Exception e)
        {
            Debug.LogError($"? 서버 연결 실패: {e.Message}");
        }
    }

    void SendLoginPacket()
    {
        // 1. 보낼 데이터 세팅
        C_LOGIN pkt = new C_LOGIN { dummyId = 7777777 };
        ushort pktSize = (ushort)(Marshal.SizeOf(typeof(PacketHeader)) + Marshal.SizeOf(typeof(C_LOGIN)));

        PacketHeader header = new PacketHeader { size = pktSize, id = 1000 }; // PKT_C_LOGIN = 1000

        // 2. 바이트 배열 할당 및 Unmanaged 메모리(포인터) 생성
        byte[] sendBuffer = new byte[pktSize];
        IntPtr ptr = Marshal.AllocHGlobal(pktSize);

        // 3. 메모리에 헤더와 데이터를 순서대로 복사 (C++의 memcpy 역할)
        Marshal.StructureToPtr(header, ptr, false);
        Marshal.StructureToPtr(pkt, ptr + Marshal.SizeOf(typeof(PacketHeader)), false);

        // 4. 완성된 메모리를 C# 바이트 배열로 빼오고 메모리 해제
        Marshal.Copy(ptr, sendBuffer, 0, pktSize);
        Marshal.FreeHGlobal(ptr);

        // 5. 서버로 전송
        _socket.Send(sendBuffer);
        Debug.Log($"?? 로그인 패킷 전송 완료! (총 {pktSize} Bytes)");
    }
}