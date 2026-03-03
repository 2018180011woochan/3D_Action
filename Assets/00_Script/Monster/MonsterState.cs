using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class MonsterState : MonoBehaviour
{
    public event Action<float> OnDamaged;

    [Header("몬스터 정보")]
    public int monsterId;
    public EMonsterType monsterType = EMonsterType.SKELETON;

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

    public void ApplyRemoteDamage(float dmg, float serverHp)
    {
        if (BossPhaseCutscene.isPlayingCutScene || isDead) return;

        currentHP = serverHp;

        if (StunEffect != null)
            StunEffect.SetActive(true);

        if (hitSfx && sfx) sfx.PlayOneShot(hitSfx, hitVolume);

        OnDamaged?.Invoke(dmg);
        onHealthChanged.Invoke(currentHP / maxHP);

        if (monsterName == "Mutant")
        {
            string[] hits = { "GetHit1", "GetHit2", "GetHit3", "GetHit4" };
            int idx = UnityEngine.Random.Range(0, hits.Length);
            animator.SetTrigger(hits[idx]);
        }
        else
        {
            animator.SetTrigger("GetHit");
        }

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (currentHP <= 0f && !isDead)
        {
            isDead = true;

            if (monsterName == "Golem")
            {
                var monAI = GetComponent<GolemAI>();
                if (monAI != null) monAI.state = GolemAI.State.Dead;

                Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

                GameObject droppedPotion = Instantiate(Potion, spawnPos, Quaternion.identity);
                if (droppedPotion.TryGetComponent(out Potion potionScript))
                    potionScript.droppedMonsterId = this.monsterId;

                if (GolemDeathSfx) sfx.PlayOneShot(GolemDeathSfx, 1f);

                if (NormalSceneGameManager.Instance != null)
                {
                    NormalSceneGameManager.Instance.OpenPortal();
                }
            }
            else if (monsterName == "Mutant")
            {
                GetComponent<MutantAI>()?.HandleDeath(5f, 1f);
                if (BossScene1Manager.Instance != null) BossScene1Manager.Instance.BossDied();
                if (MutantDeathSfx) sfx.PlayOneShot(MutantDeathSfx, 1f);
            }
            else if (monsterName == "Necromanser")
            {
                var necAI = GetComponent<NecromanserAI>();
                if (necAI != null) necAI.isDead = true;
                if (GolemDeathSfx) sfx.PlayOneShot(GolemDeathSfx, 1f);
            }
            else if (monsterName == "Zombi")
            {
                var zomAI = GetComponent<ZombiAI>();
                if (zomAI != null) zomAI.isDead = true;
                GetComponent<ZombiDissolve>()?.Play();
                if (redSkeletonDeathSfx) sfx.PlayOneShot(redSkeletonDeathSfx, 1f);
            }
            else if (monsterName == "Ghost")
            {
                var monAI = GetComponent<GhostAI>();
                if (monAI != null) monAI.state = GhostAI.State.Dead;

                Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

                GameObject droppedPotion = Instantiate(Potion, spawnPos, Quaternion.identity);
                if (droppedPotion.TryGetComponent(out Potion potionScript))
                    potionScript.droppedMonsterId = this.monsterId;

                var dissolve = GetComponent<GhostDissolve>();
                if (dissolve) dissolve.Play();
                if (BossScene1Manager.Instance != null) BossScene1Manager.Instance.ReportGhostDeath();
            }
            else
            {
                var monAI = GetComponent<MonsterAI>();
                if (monAI != null)
                {
                    monAI.currentState = EMonsterState.DEAD;
                }

                Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
                GameObject droppedPotion = Instantiate(Potion, spawnPos, Quaternion.identity);
                if (droppedPotion.TryGetComponent(out Potion potionScript))
                    potionScript.droppedMonsterId = this.monsterId;
                if (redSkeletonDeathSfx) sfx.PlayOneShot(redSkeletonDeathSfx, 1f);
            }

            animator.SetTrigger("Dead");
            if (UIManager.Instance != null) UIManager.Instance.RemoveTargetMonster(this);
        }
    }
}
