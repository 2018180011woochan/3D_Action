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

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    void Start()
    {
        InitializeInventory();
        inventoryGameObject.SetActive(false);
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
}