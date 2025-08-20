using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Item curItem;
    public Image standardImage;
    public Image curSlotImage;

    private float lastClickTime = 0f;
    private float doubleClickTime = 0.3f;
    private bool isProcessing = false;  

    private void Start()
    {
        if (curItem != null)
        {
            AddItem(curItem);
        }
    }

    public void AddItem(Item newItem)
    {
        curItem = newItem;
        curSlotImage.sprite = newItem.ItemImage;
        curSlotImage.enabled = true;
    }

    public void ClearSlot()
    {
        curItem = null;
        curSlotImage.sprite = standardImage.sprite;
        curSlotImage.enabled = false;
    }

    public void OnPointerClick(PointerEventData e)
    {
        Debug.Log("slot click"); 
        if (isProcessing) return;
        if (curItem == null) return;
        if (e.button != PointerEventData.InputButton.Left) return;


        if (e.clickCount == 2)
        {
            isProcessing = true;
            try { HandleDoubleClick(); }
            finally { isProcessing = false; }
        }
    }

    private void HandleDoubleClick()
    {
        switch (curItem.ItemName)
        {
            case "Potion":
                if (curItem.ItemObj != null)
                {
                    Debug.Log("포션클릭됨");
                    Potion potionScript = curItem.ItemObj.GetComponent<Potion>();
                    potionScript?.Heal();
                }
                ClearSlot();
                break;

            case "Armor":
            case "Sword":
            case "Shield":
            case "Bow":
                ToggleEquipment();
                break;
        }
    }

    private void ToggleEquipment()
    {
        if (curItem.isEquip)
        {
            // 장비 해제
            UnequipItem();
        }
        else
        {
            // 장비 착용
            EquipItem();
        }

        ClearSlot();
    }

    private void UnequipItem()
    {
        curItem.isEquip = false;

        switch (curItem.ItemName)
        {
            case "Armor":
                EquipManager.instance.UnEquipArmor();
                break;
            case "Sword":
                EquipManager.instance.UnEquipSword();
                break;
            case "Shield":
                EquipManager.instance.UnEquipShield();
                break;
            case "Bow":
                EquipManager.instance.UnEquipBow();
                break;
        }

        InventoryManager.instance.AddItem(curItem);
    }

    private void EquipItem()
    {
        curItem.isEquip = true;

        switch (curItem.ItemName)
        {
            case "Armor":
                EquipManager.instance.EquipArmor();
                InventoryManager.instance.EquipArmor(curItem);
                break;

            case "Sword":
                // 활이 장착되어 있으면 해제
                if (IsItemEquipped("Bow"))
                {
                    UnequipAndReturnToInventory("Bow");
                }
                EquipManager.instance.EquipSword();
                InventoryManager.instance.EquipSword(curItem);
                break;

            case "Shield":
                // 활이 장착되어 있으면 해제
                if (IsItemEquipped("Bow"))
                {
                    UnequipAndReturnToInventory("Bow");
                }
                EquipManager.instance.EquipShield();
                InventoryManager.instance.EquipShield(curItem);
                break;

            case "Bow":
                // 검과 방패가 장착되어 있으면 해제
                if (IsItemEquipped("Sword"))
                {
                    UnequipAndReturnToInventory("Sword");
                }
                if (IsItemEquipped("Shield"))
                {
                    UnequipAndReturnToInventory("Shield");
                }
                EquipManager.instance.EquipBow();
                InventoryManager.instance.EquipBow(curItem);
                break;
        }
    }

    private bool IsItemEquipped(string itemName)
    {
        // 장비 슬롯을 확인하여 해당 아이템이 장착되어 있는지 체크
        switch (itemName)
        {
            case "Armor":
                return InventoryManager.instance.armorSlot?.curItem?.ItemName == itemName;
            case "Sword":
            case "Bow":
                return InventoryManager.instance.weaponSlot?.curItem?.ItemName == itemName;
            case "Shield":
                return InventoryManager.instance.sheildSlot?.curItem?.ItemName == itemName;
        }
        return false;
    }

    private void UnequipAndReturnToInventory(string itemName)
    {
        Item item = GameDataManager.instance.itemDatabase.GetItem(itemName);
        if (item != null)
        {
            item.isEquip = false;

            // 장비 매니저에서 해제
            switch (itemName)
            {
                case "Sword":
                    EquipManager.instance.UnEquipSword();
                    InventoryManager.instance.weaponSlot?.ClearSlot();
                    break;
                case "Shield":
                    EquipManager.instance.UnEquipShield();
                    InventoryManager.instance.sheildSlot?.ClearSlot();
                    break;
                case "Bow":
                    EquipManager.instance.UnEquipBow();
                    InventoryManager.instance.weaponSlot?.ClearSlot();
                    break;
            }

            // 인벤토리에 추가
            InventoryManager.instance.AddItem(item);
        }
    }
}
