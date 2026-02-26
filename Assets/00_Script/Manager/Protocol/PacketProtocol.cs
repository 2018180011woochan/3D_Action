using System.Runtime.InteropServices;

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
    PKT_S_MONSTER_STATE = 1015,
    PKT_C_DASH = 1016, 
    PKT_S_DASH = 1017,
    PKT_C_HIT_MONSTER = 1018, 
    PKT_S_HIT_MONSTER = 1019,
    PKT_C_HIT_PLAYER = 1020, 
    PKT_S_HIT_PLAYER = 1021,
    PKT_S_SPAWN_MONSTER = 1022,
    PKT_C_USE_ITEM = 1023,        
    PKT_S_UPDATE_INVEN = 1024,    
    PKT_S_EQUIP_ITEM = 1025,
}

public enum EMonsterType : ushort
{
    NONE = 0,
    SKELETON = 1,
    GOLEM = 2,
    MUTANT = 3,
    NECROMANCER = 4,
    ZOMBIE = 5,
    GHOST = 6
}

public enum ROOM : ushort { ROOM_1, ROOM_2, ROOM_3 }
public enum AttackType : ushort { SLASH1 = 0, SLASH2, SLASH3, SKILL }
public enum EMonsterState : ushort { IDLE = 0, WANDER, CHASE, CONFRONT, ATTACK, RETREAT, DEAD }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PacketHeader { public ushort size; public ushort id; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_LOGIN { public ulong dummyId; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_LOGIN { public int playerId; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_MOVE { public float posX; public float posY; public float posZ; public float rotY; public int isRunning; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_MOVE { public int playerId; public float posX; public float posY; public float posZ; public float rotY; public int isRunning; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_ENTER_GAME { public int playerId; public float posX; public float posY; public float posZ; public float rotY; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_LEAVE_GAME { public int playerId; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_STANCE { public int isStance; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_STANCE { public int playerId; public int isStance; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_JUMP { public int dummy; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_JUMP { public int playerId; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_ATTACK { public AttackType attackType; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_ATTACK { public int playerId; public AttackType attackType; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_MONSTER_STATE
{
    public int monsterId;
    public EMonsterState state;
    public int targetId;
    public float destX;
    public float destY;
    public float destZ;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_DASH { public int dummy; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_DASH { public int playerId; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_HIT_MONSTER
{
    public int monsterId; 
    public float damage;  
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_HIT_MONSTER
{
    public int monsterId;
    public float damage;
    public float currentHp; 
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_HIT_PLAYER { public int monsterId; public float damage; public int isBlocked; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_HIT_PLAYER { public int playerId; public int monsterId; public float damage; public float currentHp; public int isBlocked; }

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_SPAWN_MONSTER
{
    public int monsterId;
    public int monsterType; 
    public float posX;
    public float posY;
    public float posZ;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct C_USE_ITEM
{
    public int slotIndex; 
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_UPDATE_INVEN
{
    public int slotIndex; 
    public int itemId;    
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct S_EQUIP_ITEM
{
    public int playerId;     
    public int equipSlot;    
    public int itemId;       
}