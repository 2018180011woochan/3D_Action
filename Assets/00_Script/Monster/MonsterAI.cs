using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("Ž�� ����")]
    public float detectionRange = 10f;  // �÷��̾� Ž�� ����
    public float attackRange = 2f;      // ���� ���� ����

    [Header("��ȸ ����")]
    public float wanderRadius = 10f;    // ��ȸ �ݰ�
    public float wanderInterval = 3f;   // ��ȸ ������ ���� �ֱ�
    public float wanderSpeed = 2f;      // ��ȸ �ӵ�

    [Header("�߰� ����")]
    public float chaseSpeed = 4f;       // �߰� �ӵ�

    [Header("��ġ ����")]
    public float confrontDuration = 2f;     // ��ġ ���� �ð�
    public float confrontRadius = 3f;       // ���� �̵� �ݰ�
    public float confrontSpeed = 60f;       // ���� �̵� �ӵ� (��/��)
    public float confrontMoveSpeed = 2f;    // ��ġ �� �̵� �ӵ�

    [Header("���� ����")]
    public float attackCooldown = 1.5f;     // ���� ��ٿ�
    public float attackLungeDistance = 1f;  // ���� �� ���� �Ÿ�
    public float attackLungeSpeed = 5f;     // ���� �� ���� �ӵ�
    public float attackLungeDuration = 0.3f; // ���� ���� �ð�

    [Header("���� ����")]
    public float retreatDistance = 3f;      // ���� �Ÿ�
    public float retreatSpeed = 3f;         // ���� �ӵ�
    public float retreatDuration = 1.5f;    // ���� �ð�

    // ������Ʈ
    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    // ���� ����
    private enum State { Idle, Wander, Chase, Confront, Attack, Cooldown, Retreat }
    private State currentState = State.Idle;

    // Ÿ�̸�
    private float wanderTimer = 0f;
    private float confrontTimer = 0f;
    private float attackCooldownTimer = 0f;
    private float retreatTimer = 0f;
    private float stateTimer = 0f;

    // �÷���
    private bool hasConfrontedOnce = false;    // ���� ��ġ�� �ߴ���
    private bool isAttackAnimating = false;     // ���� �ִϸ��̼� ������
    private bool hasTriggeredAttack = false;    // ���� Ʈ���Ÿ� ���´���
    private bool isLunging = false;             // ���� ������
    private float lungeTimer = 0f;              // ���� Ÿ�̸�

    // ��ġ ����
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

        // ���º� ó��
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
        // �÷��̾� Ž��
        if (distance <= detectionRange)
        {
            ChangeState(State.Chase);
            return;
        }

        // ��ȸ ����
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
        // Ž�� ������ ���
        if (distance > detectionRange * 1.2f)  // �ణ�� ������ ��
        {
            ChangeState(State.Wander);
            return;
        }

        // ���� ���� ����
        if (distance <= attackRange)
        {
            // ó���̸� ��ġ, �ƴϸ� �ٷ� ����
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

        // �߰�
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void ProcessConfront(float distance)
    {
        // �÷��̾ �־����� �ٽ� �߰�
        if (distance > attackRange * 1.5f)
        {
            ChangeState(State.Chase);
            return;
        }

        confrontTimer += Time.deltaTime;

        // ��ġ �ð��� ������ ����
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

        // ���� Ʈ���Ÿ� �� ���� ����
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

            // �÷��̾� �������� ����
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
            // ���� �Ϸ� �� ����
            isLunging = false;
            agent.isStopped = true;
        }

        // ���� �ִϸ��̼��� ������ ����� �ð��� �������� ��ٿ�����
        if (!isAttackAnimating && hasTriggeredAttack && stateTimer >= 1.5f)
        {
            ChangeState(State.Cooldown);
        }
    }

    void ProcessCooldown(float distance)
    {
        attackCooldownTimer += Time.deltaTime;

        // ��ٿ��� ��������
        if (attackCooldownTimer >= attackCooldown)
        {
            // �Ÿ��� ���� ���� �ൿ ����
            if (distance > attackRange)
            {
                // ���� ���� ���̸� �߰�
                ChangeState(State.Chase);
            }
            else
            {
                // ���� ���� ���̸� ����
                ChangeState(State.Retreat);
            }
        }

        // ��ٿ� �߿��� �÷��̾ �ٶ�
        LookAtPlayer();
    }

    void ProcessRetreat(float distance)
    {
        retreatTimer += Time.deltaTime;

        // ���� �ð��� �����ų� ����� �־�������
        if (retreatTimer >= retreatDuration || distance > attackRange * 1.5f)
        {
            // �ٽ� ���� ���� üũ
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

        // �÷��̾� �ݴ� �������� ����
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * retreatDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatTarget, out hit, retreatDistance * 2f, NavMesh.AllAreas))
        {
            agent.speed = retreatSpeed;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        // �����ϸ鼭�� �÷��̾ �ٶ�
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
        // ���� ���� ����
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

        // �� ���� �ʱ�ȭ
        switch (newState)
        {
            case State.Wander:
                wanderTimer = wanderInterval;  // ��� �� ������ ����
                hasConfrontedOnce = false;     // �÷��̾ ��ġ�� ��ġ ����
                agent.isStopped = false;
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                break;

            case State.Confront:
                confrontTimer = 0f;
                // �÷��̾� ���� ���ʿ��� ����
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