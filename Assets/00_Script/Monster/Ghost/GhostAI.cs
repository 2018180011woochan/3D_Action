using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("탐지/추격")]
    public float detectionRange = 12f;
    public float loseSightMultiplier = 1.25f;
    public float chaseSpeed = 3.5f;
    public float keepDistance = 6f;           // 멈추는 거리(원거리 몬스터)

    [Header("배회")]
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;
    public float wanderSpeed = 2f;

    [Header("공격")]
    public float attackInterval = 3f;
    public string attackTrigger = "Attack";
    public GameObject projectilePrefab;     

    private enum State { Wander, Chase, Dead }
    private State state = State.Wander;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    private float wanderTimer;
    private float moveThreshold = 0.05f;

    // 공격 타이머 & 캐시된 타겟 위치(애니 이벤트용)
    private float attackTimer = 0f;
    private Vector3 cachedTargetPos;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        TryWarpToNavMesh();

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        agent.stoppingDistance = keepDistance;
        state = State.Wander;
        wanderTimer = wanderInterval;
    }

    void Update()
    {
        if (state == State.Dead) return;
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
                agent.stoppingDistance = keepDistance;
                agent.isStopped = false;
                agent.SetDestination(player.position);

                // 시야 이탈하면 배회 복귀
                if (dist > detectionRange * loseSightMultiplier)
                    state = State.Wander;

                // 원하는 거리 안에 들어오면 멈춰서 바라보고, 쿨타임 끝나면 공격
                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    agent.velocity = Vector3.zero;
                    FacePlayer();
                    TryAttack();
                }
                break;
        }
    }

    void TryAttack()
    {
        if (attackTimer > 0f || IsAttackPlaying()) return;

        if (animator) animator.SetTrigger(attackTrigger);
        attackTimer = attackInterval;


        SpawnProjectile(player.position);
    }

    void SpawnProjectile(Vector3 targetPos)
    {
        if (!projectilePrefab) return;

        Instantiate(projectilePrefab, targetPos, Quaternion.identity);
    }

    bool IsAttackPlaying()
    {
        if (!animator) return false;
        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0)) return true;
        return st.IsTag("Attack") && st.normalizedTime < 0.98f;
    }

    void FacePlayer()
    {
        if (!player) return;
        Vector3 dir = player.position - transform.position; dir.y = 0f;
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
