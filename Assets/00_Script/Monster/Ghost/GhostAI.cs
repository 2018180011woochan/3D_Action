using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("탐지/추격")]
    public float detectionRange = 12f;         
    public float loseSightMultiplier = 1.25f;   
    public float chaseSpeed = 3.5f;             
    public float keepDistance = 6f;             

    [Header("배회")]
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;
    public float wanderSpeed = 2f;

    private enum State { Wander, Chase, Dead }
    private State state = State.Wander;

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;

    private float wanderTimer;
    private float moveThreshold = 0.05f;

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
                agent.stoppingDistance = keepDistance;     // 고스트는 멈추는 거리를 길게
                agent.isStopped = false;
                agent.SetDestination(player.position);

                // 시야 이탈
                if (dist > detectionRange * loseSightMultiplier)
                    state = State.Wander;

                // 원하는 거리 안에 들어오면 서서 플레이어만 바라봄
                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    agent.velocity = Vector3.zero;
                    FacePlayer();
                }
                break;
        }

        // 이동 애니메이션(옵션)
        float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
        bool isMoving = agent.hasPath && agent.remainingDistance > agent.stoppingDistance && speed > moveThreshold;
        if (animator) animator.SetBool("IsMoving", isMoving);
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
