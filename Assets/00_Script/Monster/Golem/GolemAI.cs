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


    [Header("공격")]
    public float attackRange = 3.5f;      // 이 거리 안에 들어오면 공격 시작
    public float attackCooldown = 1.0f;   // 1,2타 끝난 뒤 다음 공격까지 대기
    public string attackTrigger1 = "Attack1";
    public string attackTrigger2 = "Attack2";

    private NavMeshAgent agent;
    private Transform player;
    private Animator animator;      
    private float moveThreshold = 0.05f; 
    private float wanderTimer;
    private float attackTimer;
    public enum State { Wander, Chase, Attack, Dead }
    public State state = State.Wander;
    private int attackPhase = 0;
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
        attackTimer = 0f;
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
                agent.isStopped = false;
                agent.SetDestination(player.position);

                if (dist <= attackRange && attackTimer <= 0f && !IsAttackPlaying())
                {
                    state = State.Attack;
                    EnterAttack();     // 정지/회전
                    TriggerAttack1();  // 1타부터
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
        }
    }
}
