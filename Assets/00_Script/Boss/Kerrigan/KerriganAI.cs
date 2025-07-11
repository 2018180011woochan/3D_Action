using UnityEngine;
using UnityEngine.AI;

public class KerriganAI : MonoBehaviour
{
    [Header("���� ����")]
    public float maxHp = 100f;
    public float currentHp;

    [Header("�Ÿ� ����")]
    public float veryFarDistance = 20f;  
    public float farDistance = 10f; 
    public float closeDistance = 3f;     

    [Header("�̵� ����")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("��ġ ����")]
    public float confrontSpeed = 1.5f;  // ���� �ӵ�
    public float backStepDistance = 5f;  
    public float backStepSpeed = 2f;     
    private bool isBackStep = false;

    private float confrontTimer = 0f;   // ��ġ �����϶� � �ൿ�� ������ ���� Ÿ�̸�
    public float confrontDecisionTime = 3f;

    [Header("ű ���� ����")]
    public float kickRange = 4f;              // ű ���� ��Ÿ�
    public float kickDuration = 3f;           // ű �ִϸ��̼� ���� �ð�
    private bool isPerformingKick = false;    // ű ���� ������
    private float kickTimer = 0f;             // ű Ÿ�̸�
    private bool isMovingToKick = false;      // ű ��ġ�� �̵� ������
    public float kickApproachTimeout = 5f;
    private enum ConfrontAction
    {
        Circling,    // ����
        KickAttack,  // ű ����
        RangedAttack // ���Ÿ� ����
    }
    private ConfrontAction currentConfrontAction = ConfrontAction.Circling;

    [Header("����")]
    public Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    public enum BossState
    {
        Walk,           
        Confronting,    // ��ġ 
        Attack,         
    }

    public BossState currentState = BossState.Walk;

    void Start()
    {
        currentHp = maxHp;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = closeDistance;
        }

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (IsPhase1())
        {
            HandlePhase1Behavior(distanceToPlayer);

            if (currentState == BossState.Confronting)
            {
                HandleConfrontingBehavior();
            }
        }
        else
        {
            //HandlePhase2Behavior();
        }
        LookAtPlayer();
        UpdateAnimatorParameters();
    }

    bool IsPhase1()
    {
        return currentHp >= maxHp * 0.5f;
    }

    void HandlePhase1Behavior(float distanceToPlayer)
    {
        if (distanceToPlayer >= veryFarDistance)
        {
            ApproachPlayer();
        }
        else if (distanceToPlayer >= farDistance && distanceToPlayer < veryFarDistance)
        {
            if (currentState == BossState.Walk)
            {
                EnterConfrontState();
            }
        }
        else
        {
            //EnterAttackState();
        }
    }

    void ApproachPlayer()
    {
        currentState = BossState.Walk;

        if (agent != null && agent.enabled)
        {
            agent.SetDestination(player.position);
            agent.isStopped = false;
        }

        UpdateAnimatorParameters();
        Debug.Log("����: �߰� ���� ����!");
    }

    void EnterConfrontState()
    {
        currentState = BossState.Confronting;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // ��ġ ���� ���Խ� �ൿ ���� 
        DecideConfrontAction();
        confrontTimer = 0f;

        UpdateAnimatorParameters();
        Debug.Log("����: ��ġ ���� ����!");
    }

    void DecideConfrontAction()
    {
        if (isPerformingKick || isMovingToKick) return; // ű ������

        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                currentConfrontAction = ConfrontAction.Circling;
                Debug.Log("����: �����ϸ� ��ġ!");
                break;
            case 1:
                currentConfrontAction = ConfrontAction.KickAttack;
                Debug.Log("����: ű ���� ����! ");
                StartKickAttack();
                break;
            case 2:
                currentConfrontAction = ConfrontAction.RangedAttack;
                Debug.Log("����: ���Ÿ� ���� ����! (ű���� �׽�Ʈ�ؾߴ�ϱ� ���⵵ ű)");
                StartKickAttack();
                break;
        }
    }

    void StartKickAttack()
    {
        isMovingToKick = true;
        isPerformingKick = false;
        kickTimer = 0f;

        agent.isStopped = false;
        agent.speed = runSpeed;  
        agent.stoppingDistance = kickRange;
        agent.SetDestination(player.position);
       
    }

    void HandleConfrontingBehavior()
    {
        // ű ���� ���� �ƴ� ���� Ÿ�̸� ������Ʈ
        if (!isPerformingKick && !isMovingToKick)
        {
            confrontTimer += Time.deltaTime;

            if (confrontTimer >= confrontDecisionTime)
            {
                DecideConfrontAction();
                confrontTimer = 0f;
            }
        }

        // ���� ���õ� �ൿ ����
        switch (currentConfrontAction)
        {
            case ConfrontAction.Circling:
                HandleCirclingBehavior();
                break;
            case ConfrontAction.KickAttack:
                HandleKickAttack();
                break;
            case ConfrontAction.RangedAttack:
                // ű�׽�Ʈ�� ���� ���⵵ ű
                HandleKickAttack();
                break;
        }
    }

    void HandleKickAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // ű ��ġ�� �̵� ��
        if (isMovingToKick && !isPerformingKick)
        {
            kickTimer += Time.deltaTime;

            agent.SetDestination(player.position);

            // ��Ÿ��� �����ߴ��� Ȯ��
            if (distanceToPlayer <= kickRange)
            {
                // ű ���� ����
                isMovingToKick = false;
                isPerformingKick = true;
                kickTimer = 0f;

                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                animator.SetTrigger("Kick");
            }
            // 5�� ���� ���� ���ϸ� ����
            else if (kickTimer >= kickApproachTimeout)
            {
                Debug.Log("����: ű ���� ����! �÷��̾ �ʹ� �־���");

                // ű ���� ���
                isMovingToKick = false;
                isPerformingKick = false;
                kickTimer = 0f;
                confrontTimer = 0f;

                // agent ����
                if (agent != null)
                {
                    agent.isStopped = true;
                }

                // �ٽ� ���� ���·�
                currentConfrontAction = ConfrontAction.Circling;
            }
        }
        else if (isPerformingKick)
        {
            kickTimer += Time.deltaTime;

            if (kickTimer >= kickDuration)
            {
                isPerformingKick = false;
                kickTimer = 0f;
                confrontTimer = 0f;

                currentConfrontAction = ConfrontAction.Circling;
            }
        }
    }

    void HandleCirclingBehavior()
    {
        float currentDistance = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 Direction = Vector3.Cross(Vector3.up, directionToPlayer);

        if (currentDistance < backStepDistance)
        {
            isBackStep = true;
            Vector3 backDirection = -directionToPlayer;
            transform.position += backDirection * backStepSpeed * Time.deltaTime;
        }
        else
        {
            transform.position += Direction * confrontSpeed * Time.deltaTime;
            isBackStep = false;
        }
    }

    void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        float currentSpeed = 0f;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isMovingRight", false);
        animator.SetBool("isWalkingBack", false);

        if (currentState == BossState.Walk)
        {
            currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
            animator.SetBool("isWalking", currentSpeed > 0.1f);
        }
        else if (currentState == BossState.Confronting)
        {
            // ű ���� ���� ��
            if (currentConfrontAction == ConfrontAction.KickAttack ||
            currentConfrontAction == ConfrontAction.RangedAttack)
            {
                if (isMovingToKick)
                {
                    // ű ��ġ�� �̵� ��
                    currentSpeed = agent.velocity.magnitude;
                    animator.SetFloat("Speed", currentSpeed);
                    if (currentSpeed > 0.1f)
                    {
                        animator.SetBool("isRunning", true);
                        animator.SetBool("isWalking", false);
                    }
                }
                else if (isPerformingKick)
                {
                    // ű ���� ��
                    animator.SetFloat("Speed", 0);
                }
            }
            // ���� ���� ��
            else
            {
                animator.SetFloat("Speed", 0);
                if (isBackStep)
                    animator.SetBool("isWalkingBack", true);
                else
                    animator.SetBool("isMovingRight", true);
            }
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
}
