using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public static EquipManager instance;

    public GameObject Armor;
    public GameObject Armor2;
    public GameObject Sword;
    public GameObject Shield;
    public GameObject Bow;

    public PlayerCombat PlayerCombat;

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

        PlayerCombat.EquipSword();
    }

    public void UnEquipSword()
    {
        Sword.SetActive(false);
        PlayerCombat.UnequipWeapon();
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

        PlayerCombat.EquipBow();
    }

    public void UnEquipBow() 
    {
        Bow.SetActive(false);

        PlayerCombat.UnequipWeapon();
    }
}
