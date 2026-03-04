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

    private enum WeaponType { None, SwordAndShield, Bow }
    private WeaponType currentWeapon = WeaponType.None;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // 숫자 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QuickEquipSwordAndShield();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuickEquipBow();
        }
    }

    private void QuickEquipSwordAndShield()
    {
        if (currentWeapon == WeaponType.SwordAndShield) return;

        if (currentWeapon == WeaponType.Bow)
        {
            UnequipBow();
        }

        EquipSwordAndShield();
    }

    private void QuickEquipBow()
    {
        if (currentWeapon == WeaponType.Bow) return;

        if (currentWeapon == WeaponType.SwordAndShield)
        {
            UnequipSwordAndShield();
        }

        EquipBowWeapon();
    }

    private void UnequipBow()
    {
        //Item bowItem = GameDataManager.instance.itemDatabase.GetItem("Bow");
        /*if (bowItem != null)
        {
            //bowItem.isEquip = false;
            //InventoryManager.instance.AddItem(bowItem);
            //InventoryManager.instance.weaponSlot?.ClearSlot();
            Bow.SetActive(false);
            //PlayerCombat.UnequipWeapon();
        }*/
    }

    private void UnequipSwordAndShield()
    {
        // 검 처리
        //Item swordItem = GameDataManager.instance.itemDatabase.GetItem("Sword");
        /*if (swordItem != null)
        {
            //swordItem.isEquip = false;
            //InventoryManager.instance.AddItem(swordItem);
            //InventoryManager.instance.weaponSlot?.ClearSlot();
            Sword.SetActive(false);
        }*/

        // 방패 처리
        //Item shieldItem = GameDataManager.instance.itemDatabase.GetItem("Shield");
        /*if (shieldItem != null)
        {
            //shieldItem.isEquip = false;
            //InventoryManager.instance.AddItem(shieldItem);
            //InventoryManager.instance.sheildSlot?.ClearSlot();
            Shield.SetActive(false);
        }*/

        // PlayerCombat에 무기 해제 알림
        //PlayerCombat.UnequipWeapon();
    }

    private void EquipSwordAndShield()
    {
        // 검 장착
        //Item swordItem = GameDataManager.instance.itemDatabase.GetItem("Sword");
        /*if (swordItem != null)
        {
            RemoveItemFromInventory(swordItem);
            //swordItem.isEquip = true;
            //InventoryManager.instance.EquipSword(swordItem);
            Sword.SetActive(true);
            //PlayerCombat.EquipSword();
        }*/


        // 방패 장착
        /*Item shieldItem = GameDataManager.instance.itemDatabase.GetItem("Shield");
        if (shieldItem != null)
        {
            RemoveItemFromInventory(shieldItem);
            //shieldItem.isEquip = true;
            //InventoryManager.instance.EquipShield(shieldItem);
            Shield.SetActive(true);
        }*/

        // 현재 무기 상태 업데이트
        currentWeapon = WeaponType.SwordAndShield;
    }

    private void EquipBowWeapon()
    {
        /*Item bowItem = GameDataManager.instance.itemDatabase.GetItem("Bow");
        if (bowItem != null)
        {
            RemoveItemFromInventory(bowItem);
            //bowItem.isEquip = true;
            //InventoryManager.instance.EquipBow(bowItem);
            Bow.SetActive(true);
            //PlayerCombat.EquipBow();
            currentWeapon = WeaponType.Bow;
        }*/
    }

    private void RemoveItemFromInventory(Item item)
    {
        //var slots = InventoryManager.instance.GetInventorySlots();
/*        foreach (var slot in slots)
        {
            if (slot.curItem != null && slot.curItem.ItemName == item.ItemName)
            {
                slot.ClearSlot();
                break;
            }
        }*/
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
        //PlayerCombat.EquipSword();
    }

    public void UnEquipSword()
    {
        Sword.SetActive(false);
        //PlayerCombat.UnequipWeapon();
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
        //PlayerCombat.EquipBow();
    }

    public void UnEquipBow()
    {
        Bow.SetActive(false);
        //PlayerCombat.UnequipWeapon();
    }
}