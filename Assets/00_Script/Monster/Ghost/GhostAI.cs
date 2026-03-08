using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    public GameObject projectilePrefab;
    public string attackTrigger = "Attack";
    public GameObject teleportEffectPrefab;

    private float lastAttackTime = 0f;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent != null)
        {
            agent.updateRotation = true;
            agent.isStopped = false;
        }
    }

    public void ApplyRemoteState(S_MONSTER_STATE pkt)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        int state = (int)pkt.state;
        Vector3 dest = new Vector3(pkt.destX, pkt.destY, pkt.destZ);

        if (state == 1)
        {
            agent.isStopped = false;
            agent.speed = 2.0f;
            if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else if (state == 4)
        {
            if (Time.time - lastAttackTime < 2.5f) return;
            lastAttackTime = Time.time;

            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            Vector3 dir = dest - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir);

            if (animator != null) animator.SetTrigger(attackTrigger);

            if (projectilePrefab != null)
            {
                Instantiate(projectilePrefab, dest, Quaternion.identity);
            }
        }
        else if (state == 7)
        {
            if (teleportEffectPrefab != null)
                Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

            if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                agent.Warp(dest);
            }

            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (teleportEffectPrefab != null)
                Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}