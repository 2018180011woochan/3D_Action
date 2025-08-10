using UnityEngine;
using UnityEngine.AI;

public class GolemAI : MonoBehaviour
{
    [Header("탐지/추격")]
    public float detectionRange = 12f;         // 추격 시작 거리
    public float loseSightMultiplier = 1.25f;   // 시야 이탈 여유 배수
    public float chaseSpeed = 3.5f;
    public float stoppingDistance = 2f;

    [Header("배회")]
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;
    public float wanderSpeed = 2f;


    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;      
    private float moveThreshold = 0.05f; 
    private float wanderTimer;

    private enum State { Wander, Chase }
    private State state = State.Wander;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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
    }

    void Update()
    {
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

                if (dist > detectionRange * loseSightMultiplier)
                    state = State.Wander;
                break;
        }

        float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
        bool isMoving = agent.hasPath && agent.remainingDistance > agent.stoppingDistance && speed > moveThreshold;

        animator.SetBool("IsMoving", isMoving);
    }

    void SetRandomWanderDestination()
    {
        Vector3 random = transform.position + Random.insideUnitSphere * wanderRadius;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(random, out hit, 4f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void TryWarpToNavMesh()
    {
        if (agent && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                Debug.LogWarning($"{name}: No NavMesh near spawn!");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}
