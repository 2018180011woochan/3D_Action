using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private MonsterState ms;

    public EMonsterState currentState = EMonsterState.IDLE;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        ms = GetComponent<MonsterState>();
    }

    void Update()
    {
        if (ms != null && ms.isDead) return;

        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    public void ApplyRemoteState(S_MONSTER_STATE pkt)
    {
        if (ms != null && ms.isDead) return;
        if (currentState == EMonsterState.DEAD) return;

        currentState = pkt.state;

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        switch (pkt.state)
        {
            case EMonsterState.IDLE:
                agent.isStopped = true;
                break;

            case EMonsterState.WANDER:
                agent.isStopped = false;
                Vector3 dest = new Vector3(pkt.destX, pkt.destY, pkt.destZ);

                if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 20f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                break;

            case EMonsterState.CHASE:
                agent.isStopped = false;
                if (NetworkManager.Instance._players.TryGetValue(pkt.targetId, out GameObject targetUser))
                {
                    agent.SetDestination(targetUser.transform.position);
                }
                break;

            case EMonsterState.ATTACK:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                if (agent.isOnNavMesh) agent.ResetPath();

                if (pkt.targetId != -1 && NetworkManager.Instance._players.TryGetValue(pkt.targetId, out GameObject tgt))
                {
                    Vector3 lookDir = tgt.transform.position - transform.position;
                    lookDir.y = 0f;

                    if (lookDir.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.LookRotation(lookDir);
                    }
                }

                animator.SetTrigger("Attack");
                break;

            case EMonsterState.DEAD:
                agent.isStopped = true;
                break;
        }
    }
}