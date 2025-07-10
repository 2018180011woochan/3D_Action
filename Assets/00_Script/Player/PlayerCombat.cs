using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using Drakkar.GameUtils;
using UnityEngine.Timeline;
using Unity.Cinemachine;

public class PlayerCombat : MonoBehaviour
{
    [Header("콤보 설정")]
    public int maxCombo = 3;               // 최대 콤보 단계
    public float comboResetTime = 1f;      // 마지막 공격 후 이 시간 안에 클릭하면 콤보 이어감

    private int comboStep = 0;             // 현재 콤보 단계 (0=Idle, 1~3=공격1~3)
    private float comboTimer = 0f;         // 단계 유지 용 타이머

    [Header("Attack1 관련")]
    public float attack1MoveDuration = 0.3f;
    public float attack1MoveDistance = 0.5f;

    [Header("Attack2 관련")]
    public float attack2SpinDuration = 0.6f;
    public float attack2ForwardDistance = 3f;

    [Header("Attack3 관련")]
    public float attack3Duration = 0.8f;
    public float attack3JumpHeight = 1.5f;
    public float attack3ForwardDistance = 4f;

    [Header("이펙트 세팅")]
    public GameObject slashEffectPrefab;   
    public GameObject FinalAttackEffectPrefab;   
    public Transform swordTransform;

    private Animator animator;

    private Coroutine attackCoroutine;
    public DrakkarTrail swordTrail;

    [Header("불꽃 스킬")]
    public GameObject FireSkillEfecctPrefab;
    public bool fireSkill = false;
    private bool fireSkillOnCooldown = false;   
    private float fireSkillDuration = 10f;      
    private float fireSkillCoolTime = 20f;

    [Header("발도술")]
    public bool battoSkill = false;
    private bool battoSkillOnCooldown = false;
    private float battoSkillDuration = 3f;
    private float battoSkillCoolTime = 20f;
    public GameObject BattoEffect1;
    public GameObject BattoEffect2;
    public GameObject BattoEffect3;
    public GameObject BattoAttackEffect;
    public GameObject BattoSwordEffect;
    
    private GameObject spawnedBattoEffect1;
    private GameObject spawnedBattoEffect2;
    private GameObject spawnedBattoEffect3;

    [Header("컷씬 설정")]
    public PlayableDirector skillCutsceneDirector;
    public CinemachineCamera mainCamera;
    public CinemachineCamera skillCutsceneCamera;
    private bool isPlayingCutscene = false;

    [Header("Timeline Assets")]
    public TimelineAsset fireSkillTimeline;    // 불꽃 스킬 컷씬 타임라인
    public TimelineAsset battoSkillTimeline;    // 발도 스킬 컸씬 타임라인



    void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (skillCutsceneDirector != null)
        {
            skillCutsceneDirector.stopped += OnCutsceneComplete;
        }
    }

    void Update()
    {
        if (isPlayingCutscene)
            return;

        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            return;

        if (comboTimer > 0f)
            comboTimer -= Time.deltaTime;
        else
            comboStep = 0;  

        if (Input.GetMouseButtonDown(0))
        {
            HandleComboClick();
        }

        if (Input.GetMouseButtonDown(1))    
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Block"))
                HandleBlock();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartSkillCutscene();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartSkill2Cutscene();
        }
    }

    void StartSkillCutscene()
    {
        if (isPlayingCutscene) return;
        if (fireSkillOnCooldown) return;

        UIManager.Instance.StartFireSkillCooldown(fireSkillCoolTime);
        isPlayingCutscene = true;
        fireSkillOnCooldown = true;

        animator.SetTrigger("Skill1");
        fireSkill = true;
        FireSkillEfecctPrefab.SetActive(true);

        if (mainCamera != null && skillCutsceneCamera != null)
        {
            mainCamera.Priority = 0;
            skillCutsceneCamera.Priority = 10;
        }

        if (skillCutsceneDirector != null && fireSkillTimeline != null)
        {
            skillCutsceneDirector.playableAsset = fireSkillTimeline;
            skillCutsceneDirector.Play();
        }

        StartCoroutine(Skill1(fireSkillDuration));
        StartCoroutine(FireSkillCooldown(fireSkillCoolTime));
    }

    void StartSkill2Cutscene()
    {
        if (isPlayingCutscene) return;
        if (battoSkillOnCooldown) return;

        // 나중에
        UIManager.Instance.StartBattoSkillCooldown(battoSkillCoolTime);
        isPlayingCutscene = true;
        battoSkillOnCooldown = true;

        Vector3 forwardDir = transform.forward;
        Quaternion rot = Quaternion.LookRotation(forwardDir, Vector3.up);

        spawnedBattoEffect1 = Instantiate(BattoEffect1, transform.position, rot);
        spawnedBattoEffect2 = Instantiate(BattoEffect2, transform.position, rot);
        spawnedBattoEffect3 = Instantiate(BattoEffect3, transform.position, rot);

        animator.SetTrigger("Skill2Ready");
        battoSkill = true;

        if (mainCamera != null && skillCutsceneCamera != null)
        {
            mainCamera.Priority = 0;
            skillCutsceneCamera.Priority = 10;
        }

        if (skillCutsceneDirector != null && battoSkillTimeline != null)
        {
            skillCutsceneDirector.playableAsset = battoSkillTimeline;
            skillCutsceneDirector.Play();
        }

        StartCoroutine(Skill2(battoSkillDuration));
        StartCoroutine(battoSkillCooldown(battoSkillCoolTime));
    }

    private void OnCutsceneComplete(PlayableDirector director)
    {
        if (director == skillCutsceneDirector)
        {
            isPlayingCutscene = false;

            // 메인 카메라로 복귀
            if (mainCamera != null && skillCutsceneCamera != null)
            {
                mainCamera.Priority = 10;
                skillCutsceneCamera.Priority = 0;
            }
        }
    }

    void HandleComboClick()
    {
        switch (comboStep)
        {
            case 0:
                comboStep = 1;
                animator.SetTrigger("Attack1");
                break;
            case 1:
                comboStep = 2;
                animator.SetTrigger("Attack2");
                break;
            case 2:
                comboStep = 3;
                animator.SetTrigger("Attack3");
                break;
            default:
                break;
        }

        comboTimer = comboResetTime;
    }

    void HandleBlock()
    {
        animator.SetTrigger("Block");
    }

    public void StartAttack1Effect()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(Attack1(attack1MoveDuration, attack1MoveDistance));
    }

    public void StartAttack2Effect()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(Attack2(attack2SpinDuration, attack2ForwardDistance));
    }

    public void StartAttack3Effect()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(Attack3(attack3Duration, attack3JumpHeight, attack3ForwardDistance));
    }

    public void StopAttackEffect()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    IEnumerator Attack1(float duration, float distance)
    {
        Vector3 dir = transform.forward;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.Translate(dir * (distance * Time.deltaTime / duration), Space.World);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Attack2(float duration, float distance)
    {
        Vector3 dir = transform.forward;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float delta = Time.deltaTime;
            transform.Rotate(0, 360f * delta / duration, 0, Space.World);
            transform.Translate(dir * (distance * delta / duration), Space.World);
            elapsed += delta;
            yield return null;
        }
    }

    IEnumerator Attack3(float duration, float jumpHeight, float forwardDist)
    {
        Vector3 startPos = transform.position;
        Vector3 dir = transform.forward;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float delta = Time.deltaTime;
            float t = elapsed / duration;

            // 포물선 점프
            float y = 4f * jumpHeight * t * (1 - t);
            var pos = transform.position;
            pos.y = startPos.y + y;
            transform.position = new Vector3(pos.x, pos.y, pos.z);

            // 회전 + 전진
            transform.Rotate(0, 360f * delta / duration, 0, Space.World);
            transform.Translate(dir * (forwardDist * delta / duration), Space.World);

            elapsed += delta;
            yield return null;
        }

        // 안전하게 Y 복원
        var end = transform.position;
        end.y = startPos.y;
        transform.position = end;
    }

    public void Attack1Effect()
    {
        Vector3 forwardDir = transform.forward;

        Quaternion rot = Quaternion.LookRotation(forwardDir, Vector3.up);

        Instantiate(
            slashEffectPrefab,
            swordTransform.position,
            rot
        );
    }

    public void Attack2Effect()
    {
        Vector3 basePos = transform.position;                    
        Quaternion baseRot = Quaternion.LookRotation(transform.forward, Vector3.up);
        float angleStep = 360f / 4;

        for (int i = 0; i < 4; i++)
        {
            Quaternion dirRot = baseRot * Quaternion.Euler(0f, i * angleStep, 0f);
            Vector3 dir = dirRot * Vector3.forward;               

            Vector3 spawnPos = basePos + dir * 1f;

            Instantiate(
                slashEffectPrefab,
                spawnPos,
                dirRot
            );
        }
    }

    public void Attack3Effect()
    {
        Vector3 forwardDir = transform.forward;

        Quaternion rot = Quaternion.LookRotation(forwardDir, Vector3.up);

        Instantiate(
            FinalAttackEffectPrefab,
            swordTransform.position,
            rot
        );
    }

    IEnumerator Skill1(float time)
    {
        yield return new WaitForSeconds(time);

        fireSkill = false;
        FireSkillEfecctPrefab.SetActive(false);
    }

    IEnumerator Skill2(float time)
    {
        // 컷씬 재생 대기
        yield return new WaitForSeconds(time);

        animator.SetTrigger("Skill2");
        StartCoroutine(BattoSlash(0.3f, 10f));

        yield return new WaitForSeconds(0.1f);

        // 생성된 이펙트 오브젝트들 제거
        if (spawnedBattoEffect1 != null)
        {
            Destroy(spawnedBattoEffect1);
            spawnedBattoEffect1 = null;
        }
        if (spawnedBattoEffect2 != null)
        {
            Destroy(spawnedBattoEffect2);
            spawnedBattoEffect2 = null;
        }
        if (spawnedBattoEffect3 != null)
        {
            Destroy(spawnedBattoEffect3);
            spawnedBattoEffect3 = null;
        }

        battoSkill = false;
    }

    IEnumerator BattoSlash(float duration, float distance)
    {
        Vector3 startPos = transform.position;
        Vector3 dir = transform.forward;
        Vector3 targetPos = startPos + dir * distance;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = 1f - (1f - t) * (1f - t);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        Vector3 forwardDir = transform.forward;
        Quaternion rot = Quaternion.LookRotation(forwardDir, Vector3.up);

        Instantiate(BattoSwordEffect, swordTransform.position, rot);
        Instantiate(BattoAttackEffect, transform.position, transform.rotation);

        float attackRadius = 5f;
        float attackDamage = 10f; 

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Monster"))
            {
                MonsterState monster = hit.GetComponent<MonsterState>();
                if (monster != null)
                {
                    monster.TakeBatto(attackDamage);
                }
            }
        }
    }

    IEnumerator FireSkillCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);

        fireSkillOnCooldown = false;
    }

    IEnumerator battoSkillCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);

        battoSkillOnCooldown = false;
    }
}
