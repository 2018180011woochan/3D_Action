using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("탐지 설정")]
    public float detectionRange = 10f;  // 플레이어 탐지 범위
    public float attackRange = 2f;      // 공격 가능 범위

    [Header("배회 설정")]
    public float wanderRadius = 10f;    // 배회 반경
    public float wanderInterval = 3f;   // 배회 목적지 변경 주기
    public float wanderSpeed = 2f;      // 배회 속도

    [Header("추격 설정")]
    public float chaseSpeed = 4f;       // 추격 속도

    [Header("대치 설정")]
    public float confrontDuration = 2f;     // 대치 지속 시간
    public float confrontRadius = 3f;       // 원형 이동 반경
    public float confrontSpeed = 60f;       // 원형 이동 속도 (도/초)
    public float confrontMoveSpeed = 2f;    // 대치 중 이동 속도

    [Header("공격 설정")]
    public float attackCooldown = 1.5f;     // 공격 쿨다운
    public float attackLungeDistance = 1f;  // 공격 시 전진 거리
    public float attackLungeSpeed = 5f;     // 공격 시 전진 속도
    public float attackLungeDuration = 0.3f; // 공격 전진 시간

    [Header("후퇴 설정")]
    public float retreatDistance = 3f;      // 후퇴 거리
    public float retreatSpeed = 3f;         // 후퇴 속도
    public float retreatDuration = 1.5f;    // 후퇴 시간

    // 컴포넌트
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    // 상태 관리
    private enum State { Idle, Wander, Chase, Confront, Attack, Cooldown, Retreat }
    private State currentState = State.Idle;

    // 타이머
    private float wanderTimer = 0f;
    private float confrontTimer = 0f;
    private float attackCooldownTimer = 0f;
    private float retreatTimer = 0f;
    private float stateTimer = 0f;

    // 플래그
    private bool hasConfrontedOnce = false;    // 최초 대치를 했는지
    private bool isAttackAnimating = false;     // 공격 애니메이션 중인지
    private bool hasTriggeredAttack = false;    // 공격 트리거를 보냈는지
    private bool isLunging = false;             // 전진 중인지
    private float lungeTimer = 0f;              // 전진 타이머

    // 대치 관련
    private float currentAngle = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        ChangeState(State.Wander);
    }

    void Update()
    {
        if (player == null) return;

        UpdateAnimationState();

        float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
        animator.SetFloat("Speed", speed);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 상태별 처리
        switch (currentState)
        {
            case State.Idle:
                ProcessIdle();
                break;
            case State.Wander:
                ProcessWander(distanceToPlayer);
                break;
            case State.Chase:
                ProcessChase(distanceToPlayer);
                break;
            case State.Confront:
                ProcessConfront(distanceToPlayer);
                break;
            case State.Attack:
                ProcessAttack(distanceToPlayer);
                break;
            case State.Cooldown:
                ProcessCooldown(distanceToPlayer);
                break;
            case State.Retreat:
                ProcessRetreat(distanceToPlayer);
                break;
        }

        stateTimer += Time.deltaTime;
    }

    void ProcessIdle()
    {
        agent.isStopped = true;
        ChangeState(State.Wander);
    }

    void ProcessWander(float distance)
    {
        // 플레이어 탐지
        if (distance <= detectionRange)
        {
            ChangeState(State.Chase);
            return;
        }

        // 배회 로직
        agent.speed = wanderSpeed;
        agent.isStopped = false;

        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval)
        {
            SetRandomWanderDestination();
            wanderTimer = 0f;
        }
    }

    void ProcessChase(float distance)
    {
        // 탐지 범위를 벗어남
        if (distance > detectionRange * 1.2f)  // 약간의 여유를 둠
        {
            ChangeState(State.Wander);
            return;
        }

        // 공격 범위 도달
        if (distance <= attackRange)
        {
            // 처음이면 대치, 아니면 바로 공격
            if (!hasConfrontedOnce)
            {
                ChangeState(State.Confront);
            }
            else
            {
                ChangeState(State.Attack);
            }
            return;
        }

        // 추격
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void ProcessConfront(float distance)
    {
        // 플레이어가 멀어지면 다시 추격
        if (distance > attackRange * 1.5f)
        {
            ChangeState(State.Chase);
            return;
        }

        confrontTimer += Time.deltaTime;

        // 대치 시간이 끝나면 공격
        if (confrontTimer >= confrontDuration)
        {
            hasConfrontedOnce = true;
            ChangeState(State.Attack);
            return;
        }

        PerformCircularMovement();

        LookAtPlayer();
    }

    void ProcessAttack(float distance)
    {
        LookAtPlayer();

        // 공격 트리거를 한 번만 보냄
        if (!hasTriggeredAttack && stateTimer >= 0.1f)
        {
            animator.SetTrigger("Attack");
            hasTriggeredAttack = true;
            isLunging = true;
            lungeTimer = 0f;
        }

        if (isLunging && lungeTimer < attackLungeDuration)
        {
            lungeTimer += Time.deltaTime;

            // 플레이어 방향으로 전진
            Vector3 lungeDirection = (player.position - transform.position).normalized;
            lungeDirection.y = 0; 

            Vector3 lungeTarget = transform.position + lungeDirection * attackLungeDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(lungeTarget, out hit, attackLungeDistance * 2f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.speed = attackLungeSpeed;
                agent.SetDestination(hit.position);
            }
        }
        else if (lungeTimer >= attackLungeDuration)
        {
            // 전진 완료 후 정지
            isLunging = false;
            agent.isStopped = true;
        }

        // 공격 애니메이션이 끝났고 충분한 시간이 지났으면 쿨다운으로
        if (!isAttackAnimating && hasTriggeredAttack && stateTimer >= 1.5f)
        {
            ChangeState(State.Cooldown);
        }
    }

    void ProcessCooldown(float distance)
    {
        attackCooldownTimer += Time.deltaTime;

        // 쿨다운이 끝났으면
        if (attackCooldownTimer >= attackCooldown)
        {
            // 거리에 따라 다음 행동 결정
            if (distance > attackRange)
            {
                // 공격 범위 밖이면 추격
                ChangeState(State.Chase);
            }
            else
            {
                // 공격 범위 안이면 후퇴
                ChangeState(State.Retreat);
            }
        }

        // 쿨다운 중에도 플레이어를 바라봄
        LookAtPlayer();
    }

    void ProcessRetreat(float distance)
    {
        retreatTimer += Time.deltaTime;

        // 후퇴 시간이 끝났거나 충분히 멀어졌으면
        if (retreatTimer >= retreatDuration || distance > attackRange * 1.5f)
        {
            // 다시 공격 범위 체크
            if (distance <= attackRange)
            {
                ChangeState(State.Attack);
            }
            else
            {
                ChangeState(State.Chase);
            }
            return;
        }

        // 플레이어 반대 방향으로 후퇴
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * retreatDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatTarget, out hit, retreatDistance * 2f, NavMesh.AllAreas))
        {
            agent.speed = retreatSpeed;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        // 후퇴하면서도 플레이어를 바라봄
        LookAtPlayer();
    }

    void PerformCircularMovement()
    {
        currentAngle += confrontSpeed * Time.deltaTime;

        float radian = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(radian) * confrontRadius,
            0,
            Mathf.Sin(radian) * confrontRadius
        );

        Vector3 targetPosition = player.position + offset;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, confrontRadius * 2f, NavMesh.AllAreas))
        {
            agent.speed = confrontMoveSpeed;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    void SetRandomWanderDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void UpdateAnimationState()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsTag("Attack"))
        {
            isAttackAnimating = true;
            if (!isLunging)
            {
                agent.isStopped = true;
            }
        }
        else
        {
            isAttackAnimating = false;
        }
    }

    void ChangeState(State newState)
    {
        // 이전 상태 정리
        switch (currentState)
        {
            case State.Confront:
                animator.ResetTrigger("LeftMove");
                break;
            case State.Attack:
                hasTriggeredAttack = false;
                break;
        }

        currentState = newState;
        stateTimer = 0f;

        Debug.Log($"[{Time.time:F2}] State: {currentState}");

        // 새 상태 초기화
        switch (newState)
        {
            case State.Wander:
                wanderTimer = wanderInterval;  // 즉시 새 목적지 설정
                hasConfrontedOnce = false;     // 플레이어를 놓치면 대치 리셋
                agent.isStopped = false;
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                break;

            case State.Confront:
                confrontTimer = 0f;
                // 플레이어 기준 왼쪽에서 시작
                Vector3 toPlayer = player.position - transform.position;
                currentAngle = Mathf.Atan2(toPlayer.z, toPlayer.x) * Mathf.Rad2Deg + 90f;
                animator.SetTrigger("LeftMove");
                break;

            case State.Attack:
                agent.isStopped = true;
                isAttackAnimating = false;
                hasTriggeredAttack = false;
                isLunging = false;
                lungeTimer = 0f;
                break;

            case State.Cooldown:
                attackCooldownTimer = 0f;
                agent.isStopped = true;
                break;

            case State.Retreat:
                retreatTimer = 0f;
                agent.speed = retreatSpeed;
                agent.isStopped = false;
                break;
        }
    }
}