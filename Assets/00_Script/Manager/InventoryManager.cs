using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public GameObject slotPrefab;
    public Transform itemSlotsGameObject;
    public int inventorySize = 16;
    private List<ItemSlot> slots = new List<ItemSlot>();

    // 인벤토리 전체
    public GameObject inventoryGameObject;
    private bool isInventoryOpen = false;

    // 장비 슬롯
    public ItemSlot armorSlot;
    public ItemSlot weaponSlot;
    public ItemSlot sheildSlot;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("InventoryManager 인스턴스 생성됨");
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InitializeInventory();
        inventoryGameObject.SetActive(false);

        if (GameDataManager.instance != null)
        {
            StartCoroutine(LoadDataAfterInit());
        }

        const string potionName = "Potion";
        if (HasItem(potionName)) return;

        //// test
        var db = GameDataManager.instance?.itemDatabase;
        var potion = db?.GetItem(potionName);
        if (potion != null) AddItem(potion);
        else Debug.LogWarning($"'{potionName}' 아이템을 DB에서 찾을 수 없습니다.");

    }

    IEnumerator LoadDataAfterInit()
    {
        yield return null;  // 한 프레임 대기
        GameDataManager.instance.LoadInventory(this);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }


    void InitializeInventory()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, itemSlotsGameObject);
            ItemSlot slot = newSlot.GetComponent<ItemSlot>();
            slots.Add(slot);
        }
    }

    public bool AddItem(Item item)
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.curItem == null)
            {
                Debug.Log(item.ItemName);
                slot.AddItem(item);
                return true;
            }
        }
        return false;
    }

    public void EquipArmor(Item armor)
    {
        armorSlot.AddItem(armor);
    }

    public void EquipSword(Item sword)
    {
        weaponSlot.AddItem(sword);
    }

    public void EquipBow(Item bow)
    {
        weaponSlot.AddItem(bow);
    }

    public void EquipShield(Item shield)
    {
        sheildSlot.AddItem(shield);
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryGameObject.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            ActiveInventory();
        }
        else
        {
            CloseInventoryState();
        }
    }

    public void ActiveInventory()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;  
        inventoryGameObject.SetActive(false);
        CloseInventoryState();
    }

    private void CloseInventoryState()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }

    // 씬전환 전에 호출
    public void SaveData()
    {
        List<Item> items = new List<Item>();

        Debug.Log($"저장할 슬롯 수: {slots.Count}");

        foreach (ItemSlot slot in slots)
        {
            if (slot.curItem != null)
            {
                items.Add(slot.curItem);
                Debug.Log($"저장: {slot.curItem.ItemName}");
            }
        }

        if (GameDataManager.instance != null)
        {
            GameDataManager.instance.SaveInventory(items);
        }
        else
        {
            Debug.LogError("GameDataManager.instance가 null입니다!");
        }
    }

    public List<ItemSlot> GetInventorySlots()
    {
        return slots;
    }

    // 특정 아이템이 인벤토리에 있는지 확인하는 메서드 (선택사항)
    public bool HasItem(string itemName)
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.curItem != null && slot.curItem.ItemName == itemName)
            {
                return true;
            }
        }
        return false;
    }
}