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
        else if (distanceToPlayer >= farDistance)
        {
            //EnterConfrontState();
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

        Debug.Log("보스: 플레이어를 향해 걸어가는 중...");
    }

    void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        float currentSpeed = 0f;

        if (agent != null && currentState == BossState.Walk)
        {
            currentSpeed = agent.velocity.magnitude;
        }

        // 애니메이터 파라미터 설정
        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("isWalking", currentSpeed > 0.1f);
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
