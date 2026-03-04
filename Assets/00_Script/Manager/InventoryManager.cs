using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;
    public GameObject slotPrefab;
    public Transform itemSlotsGameObject;
    public int inventorySize = 16;
    private List<ItemSlot> slots = new List<ItemSlot>();

    public GameObject inventoryGameObject;
    private bool isInventoryOpen = false;
    public ItemDatabase itemDatabase;
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeInventory();
        inventoryGameObject.SetActive(false);
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

            slot.slotIndex = i; 
            slots.Add(slot);
        }
    }

    public void UpdateSlotFromServer(int slotIndex, int itemId)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize) return;

        ItemSlot slot = slots[slotIndex];

        if (itemId == 0)
        {
            slot.ClearSlot();
        }
        else
        {
            Item itemData = itemDatabase?.GetItemById(itemId);

            if (itemData != null)
            {
                slot.AddItem(itemData);
            }
            else
            {
                Debug.LogWarning($"[인벤토리] {itemId}번 아이템을 도감에서 찾을 수 없습니다!");
            }
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryGameObject.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}