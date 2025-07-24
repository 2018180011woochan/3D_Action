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

/*    void Start()
    {
        standardImage.sprite = curSlotImage.sprite;
    }*/

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
                Debug.Log("포션 아이템 사용");
                ClearSlot();
            }
        }

        lastClickTime = Time.time;
    }
}
