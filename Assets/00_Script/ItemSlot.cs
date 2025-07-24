using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Item curItem;
    public Image standardImage;
    public Image curSlotImage;

    void Start()
    {
        standardImage = curSlotImage;
    }

    public void AddItem(Item newItem)
    {
        curItem = newItem;
        curSlotImage.sprite = newItem.ItemImage;
    }

    public void ClearSlot()
    {
        curItem = null;
        curSlotImage = standardImage;
    }
}
