using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;

    // 아이템 데이터베이스 
    public ItemDatabase itemDatabase;

    // 저장할 데이터들
    public float playerHP = 100f;
    public List<string> inventoryItems = new List<string>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SavePlayerHP(float currentHP)
    {
        playerHP = currentHP;
        Debug.Log($"체력 저장됨" + playerHP);
    }

    public void SaveInventory(List<Item> items)
    {
        inventoryItems.Clear();

        foreach (Item item in items)
        {
            inventoryItems.Add(item.ItemName);
        }

        Debug.Log($"아이템 {inventoryItems.Count}개 저장됨");
    }

    public void LoadPlayerHP(PlayerState playerState)
    {
        if (playerState != null)
        {
            playerState.SetHP(playerHP);
            Debug.Log("체력 불러오기 완료");
        }
    }

    public void LoadInventory(InventoryManager inventory)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemDatabase가 할당되지 않았습니다!");
            return;
        }

        foreach (string itemName in inventoryItems)
        {
            Item item = itemDatabase.GetItem(itemName);
            if (item != null)
            {
                inventory.AddItem(item);
                Debug.Log($"{itemName} 로드 완료");
            }
            else
            {
                Debug.LogWarning($"{itemName}을(를) 데이터베이스에서 찾을 수 없습니다!");
            }
        }

        Debug.Log("인벤토리 불러오기 완료");
    }
}
