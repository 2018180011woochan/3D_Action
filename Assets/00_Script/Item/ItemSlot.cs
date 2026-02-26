using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public Item curItem;
    public Image standardImage;
    public Image curSlotImage;
    public int slotIndex;

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
        if (curItem == null) return;

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SendUseItemPacket(this.slotIndex);
        }
    }
}