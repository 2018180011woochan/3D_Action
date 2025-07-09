using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("플레이어 탐지 반경")]
    public float detectionRange = 5f;
    [Header("사정 거리")]
    public float attackRange = 2f;
    [Header("공격 쿨다운")]
    public float attackCooldown = 1f;


    [Header("배회 반경 & 주기")]
    public float wanderRadius = 5f;
    public float wanderTimer = 4f;

    [Header("속도 세팅")]
    public float wanderSpeed = 2f;  
    public float chaseSpeed = 4f;

    [Header("탐지할 타겟 플레이어")]
    public Transform playerTransform;

    NavMeshAgent agent;
    Animator animator;
    AnimatorStateInfo stateInfo;
    float timer;    // 배회 타이머
    float attackTimer;  // 공격 타이머 
    public float leftMoveDuration = 2f;
    enum MonsterState
    {
        Wander,
        MovingLeft,
        Chase,
        Attack
    }

    MonsterState currentState = MonsterState.Wander;
    float leftMoveTimer = 0f;
    Vector3 leftMoveStartPos;
    Vector3 leftMoveDirection;
    bool hasMovedLeft = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        timer = wanderTimer;
        attackTimer = 0f;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsTag("Attack"))
        {
            agent.isStopped = true;
            return;
        }

        float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
        animator.SetFloat("Speed", speed);

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case MonsterState.Wander:
                HandleWanderState(dist);
                break;
            case MonsterState.MovingLeft:
                HandleMovingLeftState(dist);
                break;
            case MonsterState.Chase:
                HandleChaseState(dist);
                break;
            case MonsterState.Attack:
                HandleAttackState(dist);
                break;
        }
    }

    void HandleWanderState(float dist)
    {
        if (dist <= attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        if (dist <= detectionRange && !hasMovedLeft)
        {
            StartLeftMove();
            return;
        }

        // 감지 범위 안에 있고 이미 왼쪽 이동을 했다면 추격
        if (dist <= detectionRange && hasMovedLeft)
        {
            hasMovedLeft = false;
            return;
        }

        // 배회 로직
        agent.isStopped = false;
        agent.speed = wanderSpeed;
        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0f;
        }
    }

    void HandleMovingLeftState(float dist)
    {
        leftMoveTimer += Time.deltaTime;

        // 공격 범위 안에 들어오면 즉시 공격
        if (dist <= attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        // 2초간 왼쪽으로 이동
        if (leftMoveTimer < leftMoveDuration)
        {
            // NavMeshAgent를 사용하여 왼쪽으로 이동
            Vector3 targetPos = leftMoveStartPos + leftMoveDirection * chaseSpeed * leftMoveTimer;
            agent.SetDestination(targetPos);

            // 플레이어 방향 바라보기
            LookAtPlayer();
        }
        else
        {
            // 2초 후 공격 상태로 전환
            ChangeState(MonsterState.Attack);
        }
    }

    void HandleChaseState(float dist)
    {
        if (dist <= attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        if (dist > detectionRange)
        {
            // 플레이어를 놓쳤을 때 배회로 돌아가고 왼쪽 이동 플래그 리셋
            hasMovedLeft = false;
            ChangeState(MonsterState.Wander);
            return;
        }

        // 추격
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(playerTransform.position);
        timer = wanderTimer;
    }

    void HandleAttackState(float dist)
    {
        agent.isStopped = true;
        animator.SetFloat("Speed", 0f);

        if (attackTimer <= 0f)
        {
            animator.SetTrigger("Attack");
            attackTimer = attackCooldown;
        }

        // 공격 후 상태 결정
        if (attackTimer <= attackCooldown - 0.5f) // 공격 후 잠시 대기
        {
            if (dist <= detectionRange)
            {
                ChangeState(MonsterState.Chase);
            }
            else
            {
                hasMovedLeft = false;
                ChangeState(MonsterState.Wander);
            }
        }
    }

    void StartLeftMove()
    {
        ChangeState(MonsterState.MovingLeft);
        leftMoveTimer = 0f;
        leftMoveStartPos = transform.position;
        hasMovedLeft = true;

        // 플레이어를 기준으로 왼쪽 방향 계산
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        leftMoveDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;

        // LeftMove 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("LeftMove");
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
    }

    void LookAtPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position);
        direction.y = 0; // Y축 회전 방지

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void ChangeState(MonsterState newState)
    {
        currentState = newState;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDir = Random.insideUnitSphere * dist + origin;
        NavMeshHit hit;
        NavMesh.SamplePosition(randDir, out hit, dist, layerMask);
        return hit.position;
    }
}
