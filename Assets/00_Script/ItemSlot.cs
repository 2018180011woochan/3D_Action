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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isProcessing) return;

        if (curItem == null) return;

        float clickTime = Time.time - lastClickTime;

        if (clickTime <= doubleClickTime)
        {
            isProcessing = true; 

            try
            {
                HandleDoubleClick();
            }
            finally
            {
                isProcessing = false;  
            }
        }

        lastClickTime = Time.time;
    }

    private void HandleDoubleClick()
    {
        switch (curItem.ItemName)
        {
            case "Potion":
                if (curItem.ItemObj != null)
                {
                    Potion potionScript = curItem.ItemObj.GetComponent<Potion>();
                    potionScript?.Heal();
                }
                ClearSlot();
                break;

            case "Armor":
            case "Sword":
            case "Shield":
                ToggleEquipment();
                break;
        }
    }

    private void ToggleEquipment()
    {
        if (curItem.isEquip)
        {
            // 장비 해제
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
            }

            InventoryManager.instance.AddItem(curItem);
        }
        else
        {
            // 장비 착용
            curItem.isEquip = true;

            switch (curItem.ItemName)
            {
                case "Armor":
                    EquipManager.instance.EquipArmor();
                    InventoryManager.instance.EquipArmor(curItem);
                    break;
                case "Sword":
                    EquipManager.instance.EquipSword();
                    InventoryManager.instance.EquipSword(curItem);
                    break;
                case "Shield":
                    EquipManager.instance.EquipShield();
                    InventoryManager.instance.EquipShield(curItem);
                    break;
            }
        }

        ClearSlot();
    }
}
