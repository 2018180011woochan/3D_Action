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

    private enum WeaponType { None, Sword, Bow }
    private WeaponType currentWeapon = WeaponType.None;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        currentWeapon = WeaponType.Sword;
    }

    void Update()
    {
        // 숫자 키 입력 처리
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            QuickEquipSword();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            QuickEquipBow();
        }

        // UI 업데이트
        //UpdateWeaponUI();
    }

    private void QuickEquipSword()
    {

    }

    private void QuickEquipBow()
    {
        if (currentWeapon == WeaponType.Bow) return;

        // 인벤토리에서 활 아이템 찾기
        Item bowItem = GameDataManager.instance.itemDatabase.GetItem("Bow");
        if (bowItem != null)
        {
            if (bowItem.isEquip)
            {
                Debug.Log("활이 이미 장착 상태로 되어있음. 상태 초기화 후 재장착");
                bowItem.isEquip = false;
            }

        }

        if (bowItem != null)
        {
            // 검이나 방패가 장착되어 있으면 해제
            if (currentWeapon == WeaponType.Sword)
            {
                Debug.Log("현재 검장착중");
                UnEquipSword();

                Item swordItem = GameDataManager.instance.itemDatabase.GetItem("Sword");
                if (swordItem != null)
                {
                    swordItem.isEquip = false;
                    InventoryManager.instance.AddItem(swordItem);
                    InventoryManager.instance.weaponSlot?.ClearSlot();
                }

                // 방패도 확인하고 해제
                Item shieldItem = GameDataManager.instance.itemDatabase.GetItem("Shield");
                if (shieldItem != null && shieldItem.isEquip)
                {
                    UnEquipShield();
                    shieldItem.isEquip = false;
                    InventoryManager.instance.AddItem(shieldItem);
                    InventoryManager.instance.sheildSlot?.ClearSlot();
                }
            }

            // 활 장착
            EquipBow();
            bowItem.isEquip = true;
            currentWeapon = WeaponType.Bow;

            // 인벤토리에서 제거하는 코드 필요
            InventoryManager.instance.EquipBow(bowItem);
        }
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
