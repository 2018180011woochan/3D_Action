using UnityEngine;
using UnityEngine.AI;

public class GolemAI : MonoBehaviour
{
    [Header("탐지/추격")]
    public float detectionRange = 12f;
    public float loseSightMultiplier = 1.25f;
    public float chaseSpeed = 3.5f;
    public float stoppingDistance = 2f;

    [Header("배회")]
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;
    public float wanderSpeed = 2f;

    [Header("공격")]
    public float attackRange = 3.5f;
    public float attackCooldown = 1.0f;
    public string attackTrigger1 = "Attack1";
    public string attackTrigger2 = "Attack2";

    [Header("피격 정지")]
    public string[] hitTags = { "Hit" }; // 애니메이터 상태 Tag
    public float hitStopSeconds = 0.35f;          // 데미지 직후 강제 정지 시간

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;
    private float moveThreshold = 0.05f;
    private float wanderTimer;
    private float attackTimer;
    public enum State { Wander, Chase, Attack, Dead }
    public State state = State.Wander;
    private int attackPhase = 0;

    // 데미지 콜백용
    private MonsterState ms;
    private float hitStopTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        ms = GetComponent<MonsterState>();
        if (ms) ms.OnDamaged += OnDamaged;
    }

    void OnDisable()
    {
        if (ms) ms.OnDamaged -= OnDamaged;
    }

    void OnDamaged(float dmg)
    {
        hitStopTimer = Mathf.Max(hitStopTimer, hitStopSeconds);
    }

    void Start()
    {
        TryWarpToNavMesh();

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        agent.stoppingDistance = stoppingDistance;
        state = State.Wander;
        wanderTimer = wanderInterval;
        attackTimer = 0f;
    }

    void Update()
    {
        if (state == State.Dead) return;

        // === 피격 중 정지 처리 ===
        if (hitStopTimer > 0f || IsHitPlaying())
        {
            hitStopTimer -= Time.deltaTime;
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            animator.SetBool("IsMoving", false);
            return; // 이 프레임은 추가 로직 스킵
        }
        // ========================

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        float dist = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

        switch (state)
        {
            case State.Wander:
                agent.speed = wanderSpeed;
                agent.isStopped = false;

                wanderTimer += Time.deltaTime;
                if (wanderTimer >= wanderInterval || !agent.hasPath || agent.remainingDistance < 0.2f)
                {
                    SetRandomWanderDestination();
                    wanderTimer = 0f;
                }

                if (dist <= detectionRange)
                    state = State.Chase;
                break;

            case State.Chase:
                if (!player) { state = State.Wander; break; }

                agent.speed = chaseSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.position);

                if (dist <= attackRange && attackTimer <= 0f && !IsAttackPlaying())
                {
                    state = State.Attack;
                    EnterAttack();
                    TriggerAttack1();
                    attackPhase = 1;
                }

                if (dist > detectionRange * loseSightMultiplier)
                    state = State.Wander;
                break;

            case State.Attack:
                if (!player) { ExitToChase(); break; }

                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                FacePlayer();

                if (!IsAttackPlaying() && dist > attackRange * 1.15f)
                {
                    ExitToChase();
                    break;
                }

                if (attackPhase == 1 && !IsAttackPlaying())
                {
                    TriggerAttack2();
                    attackPhase = 2;
                }
                else if (attackPhase == 2 && !IsAttackPlaying())
                {
                    attackTimer = attackCooldown;
                    ExitToChase();
                }
                break;
        }

        float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
        bool isMoving = agent.hasPath && agent.remainingDistance > agent.stoppingDistance && speed > moveThreshold;
        animator.SetBool("IsMoving", isMoving);
    }

    void EnterAttack()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
        FacePlayer();
    }

    void ExitToChase()
    {
        state = State.Chase;
        agent.isStopped = false;
        attackPhase = 0;
    }

    void TriggerAttack1()
    {
        animator.ResetTrigger(attackTrigger2);
        animator.SetTrigger(attackTrigger1);
    }

    void TriggerAttack2()
    {
        animator.ResetTrigger(attackTrigger1);
        animator.SetTrigger(attackTrigger2);
    }

    bool IsAttackPlaying()
    {
        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0)) return true;
        return st.IsTag("Attack") && st.normalizedTime < 0.98f;
    }

    bool IsHitPlaying()
    {
        if (!animator) return false;
        var st = animator.GetCurrentAnimatorStateInfo(0);
        // 현재 상태가 Hit 관련 태그인지 확인
        for (int i = 0; i < hitTags.Length; i++)
        {
            var tag = hitTags[i];
            if (!string.IsNullOrEmpty(tag) && st.IsTag(tag))
                return st.normalizedTime < 0.98f;
        }
        // 태그를 안 쓰고 상태명이 "GetHit"인 경우 대비
        if (st.IsName("GetHit")) return st.normalizedTime < 0.98f;
        return false;
    }

    void FacePlayer()
    {
        if (!player) return;
        Vector3 dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
    }

    void SetRandomWanderDestination()
    {
        Vector3 random = transform.position + Random.insideUnitSphere * wanderRadius;
        if (NavMesh.SamplePosition(random, out var hit, 4f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void TryWarpToNavMesh()
    {
        if (agent && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
    }
}
