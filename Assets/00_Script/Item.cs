using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string ItemName = "item";
    public Sprite ItemImage;
    public GameObject ItemObj;
    public bool isEquip = false;
}
