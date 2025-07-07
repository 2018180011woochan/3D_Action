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
