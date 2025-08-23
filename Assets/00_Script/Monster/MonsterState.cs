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
    public GameObject StunEffect;
    public GameObject Potion;

    public AudioClip hitSfx;
    public float hitVolume = 1f;
    private AudioSource sfx;

    [Header("몬스터별 사망 사운드")]
    public AudioClip redSkeletonDeathSfx;
    public AudioClip GolemDeathSfx;
    public AudioClip MutantDeathSfx;
    void Awake()
    {
        currentHP = maxHP;
        animator = GetComponentInChildren<Animator>();
        sfx = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.loop = false;
    }

    public void TakeDamage(float dmg)
    {
        if (BossPhaseCutscene.isPlayingCutScene) return;
        if (StunEffect != null)
            StunEffect.SetActive(true);
        if (hitSfx && sfx) sfx.PlayOneShot(hitSfx, hitVolume);
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
                sfx.PlayOneShot(GolemDeathSfx, 1f);
            }
            else if (monsterName == "Mutant")
            {
                GetComponent<MutantAI>()?.HandleDeath(5f, 1f);
                BossScene1Manager.Instance?.BossDied();
                sfx.PlayOneShot(MutantDeathSfx, 1f);
            }
            else if (monsterName == "Necromanser")
            {
                GetComponent<NecromanserAI>().isDead = true;
                sfx.PlayOneShot(GolemDeathSfx, 1f);
            }
            else if (monsterName == "Zombi")
            {
                GetComponent<ZombiAI>().isDead = true;         
                GetComponent<ZombiDissolve>()?.Play();
                sfx.PlayOneShot(redSkeletonDeathSfx, 1f);
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
                sfx.PlayOneShot(redSkeletonDeathSfx, 1f);
            }

            animator.SetTrigger("Dead");
            UIManager.Instance.RemoveTargetMonster(this);
        }
    }
}
