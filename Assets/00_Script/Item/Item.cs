using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [Header("서버 통신용 고유 번호")]
    public int itemId;           
    public EItemType itemType;   

    [Header("클라이언트 표시용")]
    public string ItemName;      
    public Sprite ItemImage;
    public GameObject ItemObj;   

}
