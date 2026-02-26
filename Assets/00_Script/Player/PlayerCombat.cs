using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class PlayerCombat : MonoBehaviour
{
    [Header("기본 설정")]
    protected Animator animator;
    protected bool isPlayingCutscene = false;

    [Header("컷씬 설정")]
    public PlayableDirector skillCutsceneDirector;
    public CinemachineCamera playerCamera;
    public CinemachineCamera skillCutsceneCamera;

    // 현재 활성화된 전투 스크립트
    private SwordCombat2 swordCombat;
    private BowCombat bowCombat;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        swordCombat = GetComponent<SwordCombat2>();
        bowCombat = GetComponent<BowCombat>();

        if (swordCombat != null) swordCombat.enabled = true;
        if (bowCombat != null) bowCombat.enabled = false;

        if (skillCutsceneDirector != null)
        {
            skillCutsceneDirector.stopped += OnCutsceneComplete;
        }
    }

    protected virtual void Update()
    {
        if (isPlayingCutscene)
            return;

        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            return;

/*        if (InventoryManager.instance.IsInventoryOpen())
            return;*/
    }

    // 무기 전환 시 호출할 메서드들
    public void EquipSword()
    {
        UIManager.Instance.SwordSkillOn();
        if (bowCombat != null && bowCombat.enabled)
        {
            bowCombat.OnWeaponUnequipped();
            bowCombat.enabled = false;
        }

        if (swordCombat != null)
        {
            swordCombat.enabled = true;
            //swordCombat.OnWeaponEquipped();
        }
    }

    public void EquipBow()
    {
        UIManager.Instance.BowSkillOn();
        if (swordCombat != null && swordCombat.enabled)
        {
            //swordCombat.OnWeaponUnequipped();
            swordCombat.enabled = false;
        }

        if (bowCombat != null)
        {
            bowCombat.enabled = true;
            bowCombat.OnWeaponEquipped();
        }
    }

    public void UnequipWeapon()
    {
        if (swordCombat != null && swordCombat.enabled)
        {
            //swordCombat.OnWeaponUnequipped();
            swordCombat.enabled = false;
        }

        if (bowCombat != null && bowCombat.enabled)
        {
            bowCombat.OnWeaponUnequipped();
            bowCombat.enabled = false;
        }
    }

    protected virtual void OnCutsceneComplete(PlayableDirector director)
    {
        if (director == skillCutsceneDirector)
        {
            isPlayingCutscene = false;

            if (playerCamera != null && skillCutsceneCamera != null)
            {
                playerCamera.Priority = 10;
                skillCutsceneCamera.Priority = 0;
            }
        }
    }

    public bool IsPlayingCutscene
    {
        get { return isPlayingCutscene; }
        set { isPlayingCutscene = value; }
    }

    public Animator GetAnimator()
    {
        return animator;
    }
}