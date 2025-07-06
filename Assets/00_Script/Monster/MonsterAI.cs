using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("�÷��̾� Ž�� �ݰ�")]
    public float detectionRange = 5f;

    [Header("��ȸ �ݰ� & �ֱ�")]
    public float wanderRadius = 5f;
    public float wanderTimer = 4f;

    [Header("Ž���� Ÿ�� �÷��̾�")]
    public Transform playerTransform;

    NavMeshAgent agent;
    Animator animator;
    float timer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        timer = wanderTimer;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
        animator.SetFloat("Speed", speed);

        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= detectionRange)
        {
            Debug.Log("�����Ÿ� �� �÷��̾� ����");
        }
        else
        {
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
