using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("�÷��̾� Ž�� �ݰ�")]
    public float detectionRange = 5f;
    [Header("���� �Ÿ�")]
    public float attackRange = 2f;
    [Header("���� ��ٿ�")]
    public float attackCooldown = 1f;


    [Header("��ȸ �ݰ� & �ֱ�")]
    public float wanderRadius = 5f;
    public float wanderTimer = 4f;

    [Header("�ӵ� ����")]
    public float wanderSpeed = 2f;  
    public float chaseSpeed = 4f;

    [Header("Ž���� Ÿ�� �÷��̾�")]
    public Transform playerTransform;

    NavMeshAgent agent;
    Animator animator;
    AnimatorStateInfo stateInfo;
    float timer;    // ��ȸ Ÿ�̸�
    float attackTimer;  // ���� Ÿ�̸� 
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

        // ���� ���� �ȿ� �ְ� �̹� ���� �̵��� �ߴٸ� �߰�
        if (dist <= detectionRange && hasMovedLeft)
        {
            hasMovedLeft = false;
            return;
        }

        // ��ȸ ����
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

        // ���� ���� �ȿ� ������ ��� ����
        if (dist <= attackRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        // 2�ʰ� �������� �̵�
        if (leftMoveTimer < leftMoveDuration)
        {
            // NavMeshAgent�� ����Ͽ� �������� �̵�
            Vector3 targetPos = leftMoveStartPos + leftMoveDirection * chaseSpeed * leftMoveTimer;
            agent.SetDestination(targetPos);

            // �÷��̾� ���� �ٶ󺸱�
            LookAtPlayer();
        }
        else
        {
            // 2�� �� ���� ���·� ��ȯ
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
            // �÷��̾ ������ �� ��ȸ�� ���ư��� ���� �̵� �÷��� ����
            hasMovedLeft = false;
            ChangeState(MonsterState.Wander);
            return;
        }

        // �߰�
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

        // ���� �� ���� ����
        if (attackTimer <= attackCooldown - 0.5f) // ���� �� ��� ���
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

        // �÷��̾ �������� ���� ���� ���
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        leftMoveDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;

        // LeftMove �ִϸ��̼� ���
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
        direction.y = 0; // Y�� ȸ�� ����

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
