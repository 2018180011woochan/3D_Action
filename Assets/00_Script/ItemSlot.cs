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
        if (curItem == null) return;

        float clickTime = Time.time - lastClickTime;

        if (clickTime <= doubleClickTime)
        {
            if (curItem.ItemName == "Potion")
            {
                // 포션을 먹으면 Heal 
                Potion potionScript = curItem.ItemObj.GetComponent<Potion>();
                potionScript.Heal();

                ClearSlot();
            }

            if (curItem.ItemName == "Armor")
            {
                if (curItem.isEquip == true)
                {
                    curItem.isEquip = false;
                    InventoryManager.instance.AddItem(curItem);
                }
                else
                {
                    curItem.isEquip = true;
                    InventoryManager.instance.EquipArmor(curItem);
                }
                ClearSlot();
            }

            if (curItem.ItemName == "Sword")
            {
                if (curItem.isEquip == true)
                {
                    curItem.isEquip = false;
                    InventoryManager.instance.AddItem(curItem);
                }
                else
                {
                    curItem.isEquip = true;
                    InventoryManager.instance.EquipSword(curItem);
                }
                ClearSlot();
            }

            if (curItem.ItemName == "Shield")
            {
                if (curItem.isEquip == true)
                {
                    curItem.isEquip = false;
                    InventoryManager.instance.AddItem(curItem);
                }
                else
                {
                    curItem.isEquip = true;
                    InventoryManager.instance.EquipShield(curItem);
                }
                ClearSlot();
            }
        }

        lastClickTime = Time.time;
    }
}
