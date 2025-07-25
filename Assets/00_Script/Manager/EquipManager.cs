using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public static EquipManager instance;

    public GameObject Armor;
    public GameObject Armor2;
    public GameObject Sword;
    public GameObject Shield;
    public GameObject Bow;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void EquipArmor()
    {
        Armor.SetActive(true);
        Armor2.SetActive(true);
    }

    public void UnEquipArmor()
    {
        Armor.SetActive(false);
        Armor2.SetActive(false);
    }

    public void EquipSword()
    {
        Sword.SetActive(true);
        Bow.SetActive(false);
    }

    public void UnEquipSword()
    {
        Sword.SetActive(false);
    }

    public void EquipShield()
    {
        Shield.SetActive(true);
    }

    public void UnEquipShield()
    {
        Shield.SetActive(false);
    }

    public void EquipBow()
    {
        Bow.SetActive(true);
        Sword.SetActive(false);
        Shield.SetActive(false);
    }

    public void UnEquipBow() 
    {
        Bow.SetActive(false);
    }
}
