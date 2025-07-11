using UnityEngine;
using UnityEngine.AI;

public class KerriganAI : MonoBehaviour
{
    [Header("보스 상태")]
    public float maxHp = 100f;
    public float currentHp;

    [Header("거리 설정")]
    public float veryFarDistance = 20f;  
    public float farDistance = 10f; 
    public float closeDistance = 3f;     

    [Header("이동 설정")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("대치 관련")]
    public float confrontSpeed = 1.5f;  // 공전 속도
    public float backStepDistance = 5f;  
    public float backStepSpeed = 2f;     
    private bool isBackStep = false;

    private float confrontTimer = 0f;   // 대치 상태일때 어떤 행동을 할지에 대한 타이머
    public float confrontDecisionTime = 3f;

    [Header("킥 공격 관련")]
    public float kickRange = 4f;              // 킥 공격 사거리
    public float kickDuration = 3f;           // 킥 애니메이션 지속 시간
    private bool isPerformingKick = false;    // 킥 동작 중인지
    private float kickTimer = 0f;             // 킥 타이머
    private bool isMovingToKick = false;      // 킥 위치로 이동 중인지
    public float kickApproachTimeout = 5f;
    private enum ConfrontAction
    {
        Circling,    // 공전
        KickAttack,  // 킥 공격
        RangedAttack // 원거리 공격
    }
    private ConfrontAction currentConfrontAction = ConfrontAction.Circling;

    [Header("참조")]
    public Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    public enum BossState
    {
        Walk,           
        Confronting,    // 대치 
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
        Debug.Log("보스: 추격 상태 진입!");
    }

    void EnterConfrontState()
    {
        currentState = BossState.Confronting;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 대치 상태 진입시 행동 결정 
        DecideConfrontAction();
        confrontTimer = 0f;

        UpdateAnimatorParameters();
        Debug.Log("보스: 대치 상태 진입!");
    }

    void DecideConfrontAction()
    {
        if (isPerformingKick || isMovingToKick) return; // 킥 공격중

        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                currentConfrontAction = ConfrontAction.Circling;
                Debug.Log("보스: 공전하며 대치!");
                break;
            case 1:
                currentConfrontAction = ConfrontAction.KickAttack;
                Debug.Log("보스: 킥 공격 선택! ");
                StartKickAttack();
                break;
            case 2:
                currentConfrontAction = ConfrontAction.RangedAttack;
                Debug.Log("보스: 원거리 공격 선택! (킥공격 테스트해야대니까 여기도 킥)");
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
        // 킥 공격 중이 아닐 때만 타이머 업데이트
        if (!isPerformingKick && !isMovingToKick)
        {
            confrontTimer += Time.deltaTime;

            if (confrontTimer >= confrontDecisionTime)
            {
                DecideConfrontAction();
                confrontTimer = 0f;
            }
        }

        // 현재 선택된 행동 실행
        switch (currentConfrontAction)
        {
            case ConfrontAction.Circling:
                HandleCirclingBehavior();
                break;
            case ConfrontAction.KickAttack:
                HandleKickAttack();
                break;
            case ConfrontAction.RangedAttack:
                // 킥테스트를 위해 여기도 킥
                HandleKickAttack();
                break;
        }
    }

    void HandleKickAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 킥 위치로 이동 중
        if (isMovingToKick && !isPerformingKick)
        {
            kickTimer += Time.deltaTime;

            agent.SetDestination(player.position);

            // 사거리에 도달했는지 확인
            if (distanceToPlayer <= kickRange)
            {
                // 킥 동작 시작
                isMovingToKick = false;
                isPerformingKick = true;
                kickTimer = 0f;

                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                animator.SetTrigger("Kick");
            }
            // 5초 동안 접근 못하면 포기
            else if (kickTimer >= kickApproachTimeout)
            {
                Debug.Log("보스: 킥 공격 포기! 플레이어가 너무 멀어짐");

                // 킥 공격 취소
                isMovingToKick = false;
                isPerformingKick = false;
                kickTimer = 0f;
                confrontTimer = 0f;

                // agent 정지
                if (agent != null)
                {
                    agent.isStopped = true;
                }

                // 다시 공전 상태로
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
            // 킥 공격 중일 때
            if (currentConfrontAction == ConfrontAction.KickAttack ||
            currentConfrontAction == ConfrontAction.RangedAttack)
            {
                if (isMovingToKick)
                {
                    // 킥 위치로 이동 중
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
                    // 킥 동작 중
                    animator.SetFloat("Speed", 0);
                }
            }
            // 공전 중일 때
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
