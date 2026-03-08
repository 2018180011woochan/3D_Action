using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("개발 테스트용 (메인 씬 다이렉트 시작)")]
    public bool isTestMode = false;
    public string testId = "testid"; 
    public string testPw = "testpw";  

    public GameObject playerPrefab;
    public Dictionary<int, GameObject> _players = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> _monsters = new Dictionary<int, GameObject>();
    public int myPlayerId;

    [Header("몬스터 프리팹")]
    public GameObject skeletonPrefab;
    public GameObject golemPrefab;
    public GameObject ghostPrefab;

    [Header("아이템 프리팹")]
    public GameObject potionPrefab;

    private Socket _socket;
    private byte[] _recvBuffer = new byte[1024];
    private List<byte> _packetBuffer = new List<byte>();

    private Queue<Action> _packetQueue = new Queue<Action>();
    private object _lock = new object();

    private Dictionary<ushort, Action<IntPtr>> _packetHandlers = new Dictionary<ushort, Action<IntPtr>>();

    private bool _isSceneLoading = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        RegisterPacketHandlers(); 
    }

    private void Start()
    {
        ConnectToServer();

        if (isTestMode)
        {
            Debug.Log($"[테스트 모드] 자동 로그인 : ID = {testId}");

            SendLoginPacket(testId, testPw);
        }
    }

    private void ConnectToServer()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            _socket.Connect("127.0.0.1", 7777);
            Debug.Log("Connected to Server.");
            //SendLoginPacket();
            _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceive, null);
        }
        catch (Exception e) { Debug.LogError($"Connection Failed: {e.Message}"); }
    }

    private void SendPacket<T>(T pkt, PacketID packetId) where T : struct
    {
        if (_socket == null || !_socket.Connected) return;

        ushort pktSize = (ushort)(Marshal.SizeOf(typeof(PacketHeader)) + Marshal.SizeOf(typeof(T)));
        PacketHeader header = new PacketHeader { size = pktSize, id = (ushort)packetId };

        byte[] sendBuffer = new byte[pktSize];
        IntPtr ptr = Marshal.AllocHGlobal(pktSize);

        Marshal.StructureToPtr(header, ptr, false);
        Marshal.StructureToPtr(pkt, ptr + Marshal.SizeOf(typeof(PacketHeader)), false);
        Marshal.Copy(ptr, sendBuffer, 0, pktSize);
        Marshal.FreeHGlobal(ptr);

        _socket.Send(sendBuffer);
    }

    public void SendLoginPacket(string id, string pw)
    {
        C_LOGIN loginPkt = new C_LOGIN
        {
            accountName = id,
            password = pw
        };

        SendPacket(loginPkt, PacketID.PKT_C_LOGIN);
    }
    public void SendMovePacket(Vector3 pos, float rotY, bool isRun) => SendPacket(new C_MOVE { posX = pos.x, posY = pos.y, posZ = pos.z, rotY = rotY, isRunning = isRun ? 1 : 0 }, PacketID.PKT_C_MOVE);
    public void SendStancePacket(bool isStance) => SendPacket(new C_STANCE { isStance = isStance ? 1 : 0 }, PacketID.PKT_C_STANCE);
    public void SendJumpPacket() => SendPacket(new C_JUMP { dummy = 1 }, PacketID.PKT_C_JUMP);
    public void SendAttackPacket(AttackType type) => SendPacket(new C_ATTACK { attackType = type }, PacketID.PKT_C_ATTACK);
    public void SendDashPacket() => SendPacket(new C_DASH { dummy = 1 }, PacketID.PKT_C_DASH);
    public void SendHitMonsterPacket(int mobId, float dmg)
        => SendPacket(new C_HIT_MONSTER { monsterId = mobId, damage = dmg }, PacketID.PKT_C_HIT_MONSTER);
    public void SendHitPlayerPacket(int mobId, float dmg, bool isBlocked)
        => SendPacket(new C_HIT_PLAYER { monsterId = mobId, damage = dmg, isBlocked = isBlocked ? 1 : 0 }, PacketID.PKT_C_HIT_PLAYER);
    public void SendUseItemPacket(int slotIdx)
    {
        C_USE_ITEM pkt = new C_USE_ITEM { slotIndex = slotIdx };
        SendPacket(pkt, PacketID.PKT_C_USE_ITEM);
    }
    public void SendPickupItemPacket(int itemId, int droppedMonsterId)
    {
        C_PICKUP_ITEM pkt = new C_PICKUP_ITEM
        {
            itemId = itemId,
            droppedMonsterId = droppedMonsterId
        };

        SendPacket(pkt, PacketID.PKT_C_PICKUP_ITEM);
    }

    public void SendEnterPortalPacket()
    {
        C_ENTER_PORTAL pkt = new C_ENTER_PORTAL { dummy = 1 };
        SendPacket(pkt, PacketID.PKT_C_ENTER_PORTAL);
    }
    private void RegisterPacketHandlers()
    {
        _packetHandlers.Add((ushort)PacketID.PKT_S_LOGIN, Handle_S_LOGIN);
        _packetHandlers.Add((ushort)PacketID.PKT_S_ENTER_GAME, Handle_S_ENTER_GAME);
        _packetHandlers.Add((ushort)PacketID.PKT_S_MOVE, Handle_S_MOVE);
        _packetHandlers.Add((ushort)PacketID.PKT_S_LEAVE_GAME, Handle_S_LEAVE_GAME);
        _packetHandlers.Add((ushort)PacketID.PKT_S_STANCE, Handle_S_STANCE);
        _packetHandlers.Add((ushort)PacketID.PKT_S_JUMP, Handle_S_JUMP);
        _packetHandlers.Add((ushort)PacketID.PKT_S_ATTACK, Handle_S_ATTACK);
        _packetHandlers.Add((ushort)PacketID.PKT_S_MONSTER_STATE, Handle_S_MONSTER_STATE);
        _packetHandlers.Add((ushort)PacketID.PKT_S_DASH, Handle_S_DASH);
        _packetHandlers.Add((ushort)PacketID.PKT_S_HIT_MONSTER, Handle_S_HIT_MONSTER);
        _packetHandlers.Add((ushort)PacketID.PKT_S_HIT_PLAYER, Handle_S_HIT_PLAYER);
        _packetHandlers.Add((ushort)PacketID.PKT_S_SPAWN_MONSTER, Handle_S_SPAWN_MONSTER);
        _packetHandlers.Add((ushort)PacketID.PKT_S_UPDATE_INVEN, Handle_S_UPDATE_INVEN);
        _packetHandlers.Add((ushort)PacketID.PKT_S_USE_ITEM, Handle_S_USE_ITEM);
        _packetHandlers.Add((ushort)PacketID.PKT_S_PICKUP_ITEM, Handle_S_PICKUP_ITEM);
        _packetHandlers.Add((ushort)PacketID.PKT_S_SPAWN_ITEM, Handle_S_SPAWN_ITEM);
        _packetHandlers.Add((ushort)PacketID.PKT_S_OPEN_PORTAL, Handle_S_OPEN_PORTAL);
        _packetHandlers.Add((ushort)PacketID.PKT_S_ENTER_PORTAL, Handle_S_ENTER_PORTAL);
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

                    if (_packetHandlers.TryGetValue(header.id, out Action<IntPtr> handler))
                    {
                        handler.Invoke(payloadPtr);
                    }
                    else Debug.LogWarning($"[경고] 처리할 수 없는 패킷 ID: {header.id}");

                    Marshal.FreeHGlobal(dataPtr);
                }
                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, OnReceive, null);
            }
        }
        catch (Exception e) { Debug.LogError($"Receive Error: {e.Message}"); }
    }

    private void Handle_S_LOGIN(IntPtr payloadPtr)
    {
        S_LOGIN pkt = (S_LOGIN)Marshal.PtrToStructure(payloadPtr, typeof(S_LOGIN));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (pkt.success == 1)
                {
                    myPlayerId = pkt.playerId;
                    Debug.Log($"로그인 성공! 내 ID: {myPlayerId}");

                    if (isTestMode)
                    {
                        Debug.Log("[테스트 모드] 씬 이동 생략! 바로 스폰 패킷 받기 대기!");
                        _isSceneLoading = false; 
                        return; 
                    }

                    _isSceneLoading = true;
                    SceneBridge.NextSceneName = "MainScene";
                    UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
                }
                else
                {
                    Debug.Log("로그인 실패! 팝업창 띄운다!");
                    LoginUI loginUI = FindObjectOfType<LoginUI>();
                    if (loginUI != null)
                    {
                        loginUI.ShowPopup("로그인 정보가 없습니다.\n아이디와 비밀번호를 확인해주세요.");
                    }
                }
            });
        }
    }

    private void Handle_S_ENTER_GAME(IntPtr payloadPtr)
    {
        S_ENTER_GAME pkt = (S_ENTER_GAME)Marshal.PtrToStructure(payloadPtr, typeof(S_ENTER_GAME));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (_players.ContainsKey(pkt.playerId)) return;
                Vector3 spawnPos = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
                GameObject go = Instantiate(playerPrefab, spawnPos, Quaternion.Euler(0, pkt.rotY, 0));
                _players.Add(pkt.playerId, go);

                bool isMine = (pkt.playerId == myPlayerId);
                if (go.TryGetComponent(out SamuraiMovement movement)) movement.isMine = isMine;

                if (isMine)
                {
                    if (GameObject.Find("PlayerCamera")?.TryGetComponent(out Unity.Cinemachine.CinemachineCamera vcam) == true)
                        vcam.Target.TrackingTarget = go.transform;
                }
            });
        }
    }

    private void Handle_S_MOVE(IntPtr payloadPtr)
    {
        S_MOVE pkt = (S_MOVE)Marshal.PtrToStructure(payloadPtr, typeof(S_MOVE));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (pkt.playerId == myPlayerId) return;
                if (_players.TryGetValue(pkt.playerId, out GameObject go) && go.TryGetComponent(out SamuraiMovement mov))
                {
                    mov.syncPos = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
                    mov.syncRotY = pkt.rotY;
                    mov.syncIsRunning = (pkt.isRunning == 1);
                }
            });
        }
    }

    private void Handle_S_LEAVE_GAME(IntPtr payloadPtr)
    {
        S_LEAVE_GAME pkt = (S_LEAVE_GAME)Marshal.PtrToStructure(payloadPtr, typeof(S_LEAVE_GAME));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (_players.TryGetValue(pkt.playerId, out GameObject go))
                {
                    Destroy(go);
                    _players.Remove(pkt.playerId);
                }
            });
        }
    }

    private void Handle_S_STANCE(IntPtr payloadPtr)
    {
        S_STANCE pkt = (S_STANCE)Marshal.PtrToStructure(payloadPtr, typeof(S_STANCE));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (pkt.playerId == myPlayerId) return;
                if (_players.TryGetValue(pkt.playerId, out GameObject go) && go.TryGetComponent(out SamuraiMovement mov))
                    mov.ApplyRemoteStance(pkt.isStance == 1);
            });
        }
    }

    private void Handle_S_JUMP(IntPtr payloadPtr)
    {
        S_JUMP pkt = (S_JUMP)Marshal.PtrToStructure(payloadPtr, typeof(S_JUMP));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (pkt.playerId == myPlayerId) return;
                if (_players.TryGetValue(pkt.playerId, out GameObject go) && go.TryGetComponent(out SamuraiMovement mov))
                    mov.ApplyRemoteJump();
            });
        }
    }

    private void Handle_S_ATTACK(IntPtr payloadPtr)
    {
        S_ATTACK pkt = (S_ATTACK)Marshal.PtrToStructure(payloadPtr, typeof(S_ATTACK));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (pkt.playerId == myPlayerId) return;
                if (_players.TryGetValue(pkt.playerId, out GameObject go) && go.TryGetComponent(out SamuraiCombat com))
                    com.ApplyRemoteAttack(pkt.attackType);
            });
        }
    }

    private void Handle_S_MONSTER_STATE(IntPtr payloadPtr)
    {
        S_MONSTER_STATE pkt = (S_MONSTER_STATE)Marshal.PtrToStructure(payloadPtr, typeof(S_MONSTER_STATE));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (_monsters.TryGetValue(pkt.monsterId, out GameObject mob))
                {
                    if (mob == null)
                    {
                        _monsters.Remove(pkt.monsterId);
                        return;
                    }

                    if (mob.TryGetComponent(out MonsterAI monAI))
                        monAI.ApplyRemoteState(pkt);
                    else if (mob.TryGetComponent(out GolemAI golemAI))
                        golemAI.ApplyRemoteState(pkt);
                    else if (mob.TryGetComponent(out GhostAI ghostAI))
                        ghostAI.ApplyRemoteState(pkt);
                }
            });
        }
    }

    private void Handle_S_DASH(IntPtr payloadPtr)
    {
        S_DASH pkt = (S_DASH)Marshal.PtrToStructure(payloadPtr, typeof(S_DASH));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (pkt.playerId == myPlayerId) return; 

                if (_players.TryGetValue(pkt.playerId, out GameObject go) && go.TryGetComponent(out SamuraiMovement mov))
                {
                    mov.ApplyRemoteDash();
                }
            });
        }
    }

    private void Handle_S_HIT_MONSTER(IntPtr payloadPtr)
    {
        S_HIT_MONSTER pkt = (S_HIT_MONSTER)Marshal.PtrToStructure(payloadPtr, typeof(S_HIT_MONSTER));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (_monsters.TryGetValue(pkt.monsterId, out GameObject mob) && mob.TryGetComponent(out MonsterState ms))
                {
                    ms.ApplyRemoteDamage(pkt.damage, pkt.currentHp);
                }
            });
        }
    }

    private void Handle_S_HIT_PLAYER(IntPtr payloadPtr)
    {
        S_HIT_PLAYER pkt = (S_HIT_PLAYER)Marshal.PtrToStructure(payloadPtr, typeof(S_HIT_PLAYER));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {

                Debug.Log($"[네트워크] {pkt.playerId}번 유저 맞음! 남은 HP: {pkt.currentHp}");
                if (_players.TryGetValue(pkt.playerId, out GameObject go) && go.TryGetComponent(out PlayerState ps))
                {
                    ps.ApplyRemoteDamage(pkt.damage, pkt.currentHp, pkt.isBlocked == 1);
                }
            });
        }
    }

    private void Handle_S_SPAWN_MONSTER(IntPtr payloadPtr)
    {
        S_SPAWN_MONSTER pkt = (S_SPAWN_MONSTER)Marshal.PtrToStructure(payloadPtr, typeof(S_SPAWN_MONSTER));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {

                if (_monsters.ContainsKey(pkt.monsterId)) return;

                GameObject prefabToSpawn = null;
                if (pkt.monsterType == (int)EMonsterType.SKELETON) prefabToSpawn = skeletonPrefab;
                else if (pkt.monsterType == (int)EMonsterType.GOLEM) prefabToSpawn = golemPrefab;
                else if (pkt.monsterType == (int)EMonsterType.GHOST) prefabToSpawn = ghostPrefab;

                if (prefabToSpawn != null)
                {
                    Vector3 spawnPos = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
                    GameObject newMob = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

                    if (newMob.TryGetComponent(out MonsterState ms))
                    {
                        ms.monsterId = pkt.monsterId;
                        ms.monsterType = (EMonsterType)pkt.monsterType;
                    }

                    _monsters[pkt.monsterId] = newMob;

                    Debug.Log($"[서버 스폰] {pkt.monsterId}번 몬스터(타입:{pkt.monsterType}) 소환 완료!");
                }
            });
        }
    }

    private void Handle_S_UPDATE_INVEN(IntPtr payloadPtr)
    {
        S_UPDATE_INVEN pkt = (S_UPDATE_INVEN)Marshal.PtrToStructure(payloadPtr, typeof(S_UPDATE_INVEN));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (InventoryManager.instance != null)
                    InventoryManager.instance.UpdateSlotFromServer(pkt.slotIndex, pkt.itemId);
            });
        }
    }

    private void Handle_S_SPAWN_ITEM(IntPtr payloadPtr)
    {
        S_SPAWN_ITEM pkt = (S_SPAWN_ITEM)Marshal.PtrToStructure(payloadPtr, typeof(S_SPAWN_ITEM));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                Vector3 spawnPos = new Vector3(pkt.posX, pkt.posY + 1f, pkt.posZ);
                GameObject go = Instantiate(potionPrefab, spawnPos, Quaternion.identity);

                if (go.TryGetComponent(out Potion p))
                {
                    p.droppedMonsterId = pkt.droppedMonsterId;
                }

                Debug.Log($"[클라 로그] {pkt.droppedMonsterId}번 몬스터가 떨군 {pkt.itemId}번 템 바닥에 스폰 완료!");
            });
        }
    }

    private void Handle_S_OPEN_PORTAL(IntPtr payloadPtr)
    {
        S_OPEN_PORTAL pkt = (S_OPEN_PORTAL)Marshal.PtrToStructure(payloadPtr, typeof(S_OPEN_PORTAL));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (pkt.isOpened == 1)
                {
                    if (NormalSceneGameManager.Instance != null) NormalSceneGameManager.Instance.OpenPortal();
                    if (BossScene1Manager.Instance != null) BossScene1Manager.Instance.BossDied();
                }
            });
        }
    }

    private void Handle_S_ENTER_PORTAL(IntPtr payloadPtr)
    {
        S_ENTER_PORTAL pkt = (S_ENTER_PORTAL)Marshal.PtrToStructure(payloadPtr, typeof(S_ENTER_PORTAL));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                _players.Clear();
                _monsters.Clear();

                string nextScene = "";

                if (pkt.destRoomId == 2) nextScene = "BossScene1";
                else if (pkt.destRoomId == 3) nextScene = "BossScene2";

                if (nextScene != "")
                {
                    Debug.Log($"[네트워크] 서버 명령 수신! {nextScene}으로 이동합니다!");
                    SceneBridge.NextSceneName = nextScene;
                    _isSceneLoading = true;
                    UnityEngine.SceneManagement.SceneManager.LoadScene("LoadingScene");
                }
            });
        }
    }
    private void Update()
    {
        lock (_lock)
        {
            if (_isSceneLoading)
            {
                string curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (curScene == "MainScene" || curScene == "BossScene1" || curScene == "BossScene2")
                {
                    _isSceneLoading = false;
                }
                else
                {
                    return; 
                }
            }

            while (_packetQueue.Count > 0)
            {
                Action action = _packetQueue.Dequeue();
                try { action.Invoke(); }
                catch (Exception e) { Debug.LogError($"[패킷 처리 에러] : {e.Message}"); }

                if (_isSceneLoading)
                {
                    break;
                }
            }
        }
    }

    private void Handle_S_PICKUP_ITEM(IntPtr payloadPtr)
    {
        S_PICKUP_ITEM pkt = (S_PICKUP_ITEM)Marshal.PtrToStructure(payloadPtr, typeof(S_PICKUP_ITEM));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                Potion[] potions = FindObjectsOfType<Potion>();
                foreach (Potion p in potions)
                {
                    if (p.droppedMonsterId == pkt.droppedMonsterId)
                    {
                        Destroy(p.gameObject);
                        break;
                    }
                }
            });
        }
    }

    private void Handle_S_USE_ITEM(IntPtr payloadPtr)
    {
        S_USE_ITEM pkt = (S_USE_ITEM)Marshal.PtrToStructure(payloadPtr, typeof(S_USE_ITEM));
        lock (_lock)
        {
            _packetQueue.Enqueue(() => {
                if (_players.TryGetValue(pkt.playerId, out GameObject go) && go.TryGetComponent(out PlayerState ps))
                {
                    ps.ApplyRemoteUseItem(pkt.itemId, pkt.currentHp);
                }
            });
        }
    }

    private void OnApplicationQuit() => Disconnect();
    private void OnDestroy() => Disconnect();
    private void Disconnect()
    {
        if (_socket != null && _socket.Connected)
        {
            try { _socket.Shutdown(SocketShutdown.Both); _socket.Close(); _socket = null; }
            catch (Exception e) { Debug.LogError($"소켓 종료 에러: {e.Message}"); }
        }
    }
}