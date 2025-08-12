using System.Collections;
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

    [Header("텔레포트")]
    public float teleportDistanceFromPlayer = 6f;
    public float teleportDelay = 1f;
    public float teleportCooldown = 1.5f;
    public GameObject teleportEffectPrefab;
    public float sampleMaxDistance = 1.0f;         
    public float ringTolerance = 0.25f;             
    private float lastTeleportTime = -999f;
    private Coroutine teleportCo;                 
    private bool teleportPending = false;         
    public enum State { Wander, Chase, Dead }
    public State state = State.Wander;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    private float wanderTimer;
    private float moveThreshold = 0.05f;

    // 공격 타이머 & 캐시된 타겟 위치(애니 이벤트용)
    private float attackTimer = 0f;
    private Vector3 cachedTargetPos;

    private MonsterState ms;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        ms = GetComponent<MonsterState>();
        ms.OnDamaged += HandleDamaged;

    }

    void OnDisable()
    {
        ms.OnDamaged -= HandleDamaged;
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

    void HandleDamaged(float dmg)
    {
        if (state == State.Dead) return;
        if (Time.time - lastTeleportTime < teleportCooldown) return;

        if (teleportPending) return;

        teleportCo = StartCoroutine(TeleportAfterDelay());
    }

    IEnumerator TeleportAfterDelay()
    {
        teleportPending = true;
        yield return new WaitForSeconds(teleportDelay);

        if (!isActiveAndEnabled || state == State.Dead)
        {
            teleportPending = false;
            teleportCo = null;
            yield break;
        }

        Teleport();
        teleportPending = false;
        teleportCo = null;
    }
    void Teleport()
    {
        lastTeleportTime = Time.time;

        Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

        if (player && TryGetPointOnRing(player.position, teleportDistanceFromPlayer, sampleMaxDistance, ringTolerance, out var newPos))
        {
            if (agent) { agent.Warp(newPos); agent.ResetPath(); }


            Instantiate(teleportEffectPrefab, newPos, Quaternion.identity);

            state = State.Chase;
            if (agent && agent.isOnNavMesh)
                agent.SetDestination(player.position);
        }
    }

    bool TryGetPointOnRing(Vector3 playerPos, float radius, float maxSampleDist, float tolerance, out Vector3 pos)
    {
        const int attempts = 24;

        for (int i = 0; i < attempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 ideal = playerPos + dir * radius;

            if (NavMesh.SamplePosition(ideal, out var hit, maxSampleDist, NavMesh.AllAreas))
            {
                float planarDist = PlanarDistance(hit.position, playerPos);
                if (Mathf.Abs(planarDist - radius) <= tolerance)
                {
                    pos = hit.position;
                    return true;
                }
            }
        }

        float wider = Mathf.Max(tolerance * 2f, 0.5f);
        for (int i = 0; i < attempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 ideal = playerPos + dir * radius;

            if (NavMesh.SamplePosition(ideal, out var hit, maxSampleDist, NavMesh.AllAreas))
            {
                float planarDist = PlanarDistance(hit.position, playerPos);
                if (Mathf.Abs(planarDist - radius) <= wider)
                {
                    pos = hit.position;
                    return true;
                }
            }
        }

        pos = transform.position;
        return false;
    }

    float PlanarDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
