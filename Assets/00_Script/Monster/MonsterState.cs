using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MonsterState : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHP = 100f;
    public string monsterName;
    public float currentHP { get; private set; }
    public bool isDead = false;

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();
    Animator animator;

    [Header("발도술 공격을 받았을 때")]
    public GameObject BattoHitEffect;
    private Coroutine battoCoroutine;

    public GameObject Potion;
    void Awake()
    {
        currentHP = maxHP;
        //currentHP = 40;
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float dmg)
    {
        Debug.Log(dmg);
        if (monsterName.ToLower() == "kerrigan")
        {
            KerriganPhase2 phase2 = GetComponent<KerriganPhase2>();
            if (phase2.enabled && phase2.IsInvincible())
            {
                Debug.Log("무적 상태");
                return;
            }
        }

        currentHP = Mathf.Max(currentHP - dmg, 0f);
        onHealthChanged.Invoke(currentHP / maxHP);
        animator.SetTrigger("GetHit");

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (currentHP <= 0f)
        {
            isDead = true;
            if (monsterName.ToLower() == "kerrigan")
            {
                // 보스 사망 
            }
            else
            {
                var monAI = GetComponent<MonsterAI>();
                monAI.currentState = MonsterAI.State.Dead;

                Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y  + 1f, transform.position.z);

                GameObject potion = Instantiate(
                    Potion,
                    spawnPos,
                    Quaternion.identity
                    );

            }

            animator.SetTrigger("Dead");
            UIManager.Instance.RemoveTargetMonster(this);
            NormalSceneGameManager.Instance.GameEndCnt++;
            Debug.Log("카운트 " + NormalSceneGameManager.Instance.GameEndCnt);
        }
    }

    public void TakeBatto(float dmg)
    {
        if (battoCoroutine != null)
            return;
        UIManager.Instance.AddTargetMonster(this);
        battoCoroutine = StartCoroutine(BattoDamageOverTime(dmg));
    }

    IEnumerator BattoDamageOverTime(float dmgPerTick)
    {
        // 발도술 히트 이펙트 생성
        GameObject hitEffect = Instantiate(
            BattoHitEffect,
            transform.position,
            Quaternion.identity
        );

        hitEffect.transform.SetParent(transform);

        // 5초 동안 1초마다 데미지
        for (int i = 0; i < 5; i++)
        {
            // 데미지 적용
            currentHP = Mathf.Max(currentHP - dmgPerTick, 0f);
            onHealthChanged.Invoke(currentHP / maxHP);

            animator.SetTrigger("GetHit");

            // 사망 체크
            if (currentHP <= 0f && !isDead)
            {
                isDead = true;
                var monAI = GetComponent<MonsterAI>();
                if (monAI != null)
                    monAI.currentState = MonsterAI.State.Dead;

                animator.SetTrigger("Dead");
                UIManager.Instance.RemoveTargetMonster(this);

                // 이펙트 즉시 제거
                if (hitEffect != null)
                    Destroy(hitEffect);

                battoCoroutine = null;
                yield break;
            }

            // 마지막 틱이 아니면 1초 대기
            if (i < 4)
                yield return new WaitForSeconds(1f);
        }

        // 3초 후 이펙트 제거
        if (hitEffect != null)
            Destroy(hitEffect);

        battoCoroutine = null;
    }
}
