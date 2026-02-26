using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Header("모든 아이템 목록")]
    public List<Item> items = new List<Item>();

    public Item GetItem(string itemName)
    {
        return items.Find(x => x.ItemName == itemName);
    }

    public Item GetItemById(int id)
    {
        return items.Find(x => x.itemId == id);
    }

    public bool HasItem(string itemName)
    {
        return items.Exists(x => x.ItemName == itemName);
    }
}