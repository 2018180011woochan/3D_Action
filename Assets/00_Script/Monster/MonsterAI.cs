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
        if (dist <= attackRange)
        {
            agent.isStopped = true;
            animator.SetFloat("Speed", 0f);

            if (attackTimer <= 0f)
            {
                animator.SetTrigger("Attack");
                attackTimer = attackCooldown;
            }
        }
        else if (dist <= detectionRange)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(playerTransform.position);
            timer = wanderTimer;
        }
        else
        {
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
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layerMask)
    {
        Vector3 randDir = Random.insideUnitSphere * dist + origin;
        NavMeshHit hit;
        NavMesh.SamplePosition(randDir, out hit, dist, layerMask);
        return hit.position;
    }
}
