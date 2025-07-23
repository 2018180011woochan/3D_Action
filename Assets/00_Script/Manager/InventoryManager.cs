using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;



    public GameObject slotPrefab;
    public Transform itemSlotsGameObject;
    public int inventorySize = 16;

    private List<ItemSlot> slots = new List<ItemSlot>();
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        InitializeInventory();
    }

    // 인벤토리 초기화
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

        // 인벤토리가 가득 참 
        return false;
    }
}
