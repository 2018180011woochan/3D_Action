using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("플레이어 탐지 반경")]
    public float detectionRange = 5f;

    [Header("배회 반경 & 주기")]
    public float wanderRadius = 5f;
    public float wanderTimer = 4f;

    [Header("탐지할 타겟 플레이어")]
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
            Debug.Log("사정거리 안 플레이어 접근");
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
