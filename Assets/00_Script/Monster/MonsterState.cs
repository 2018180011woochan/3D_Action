using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MonsterState : MonoBehaviour
{
    public event Action<float> OnDamaged;

    [Header("체력 설정")]
    public float maxHP = 100f;
    public string monsterName;
    public float currentHP { get; private set; }
    public bool isDead = false;

    public UnityEvent<float> onHealthChanged = new UnityEvent<float>();
    Animator animator;

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
        OnDamaged?.Invoke(dmg);
        onHealthChanged.Invoke(currentHP / maxHP);

        if (monsterName == "Mutant")
        {
            string[] hits = { "GetHit1", "GetHit2", "GetHit3", "GetHit4" };
            int idx = UnityEngine.Random.Range(0, hits.Length);
            animator.SetTrigger(hits[idx]);
        }
        else
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
            if (monsterName == "Golem")
            {
                var monAI = GetComponent<GolemAI>();
                monAI.state = GolemAI.State.Dead;

                Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

                GameObject potion = Instantiate(
                    Potion,
                    spawnPos,
                    Quaternion.identity
                    );
            }
            if (monsterName == "Mutant")
            {
                GetComponent<MutantAI>()?.HandleDeath(5f, 1f);
            }
            else if (monsterName == "Ghost")
            {
                var monAI = GetComponent<GhostAI>();
                monAI.state = GhostAI.State.Dead;

                Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

                GameObject potion = Instantiate(
                    Potion,
                    spawnPos,
                    Quaternion.identity
                    );
                var dissolve = GetComponent<GhostDissolve>();
                if (dissolve) dissolve.Play();
                BossScene1Manager.Instance.ReportGhostDeath();
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
        }
    }
}
