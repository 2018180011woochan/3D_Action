using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [Header("모든 아이템 목록")]
    public List<Item> items = new List<Item>();

    // 이름으로 아이템 찾기
    public Item GetItem(string itemName)
    {
        return items.Find(x => x.ItemName == itemName);
    }

    // 아이템이 데이터베이스에 있는지 확인
    public bool HasItem(string itemName)
    {
        return items.Exists(x => x.ItemName == itemName);
    }
}