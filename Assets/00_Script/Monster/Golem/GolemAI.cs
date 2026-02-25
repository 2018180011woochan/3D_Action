using UnityEngine;
using UnityEngine.AI;

public class GolemAI : MonoBehaviour
{
    [Header("이동 속도")]
    public float chaseSpeed = 3.5f;
    public float wanderSpeed = 2f;

    [Header("공격")]
    public string attackTrigger1 = "Attack1";
    public string attackTrigger2 = "Attack2";

    [Header("피격 정지 (역경직)")]
    public string[] hitTags = { "Hit" };
    public float hitStopSeconds = 0.35f;

    private NavMeshAgent agent;
    private Animator animator;
    private float moveThreshold = 0.05f;

    public enum State { Wander, Chase, Attack, Dead }
    public State state = State.Wander;

    private MonsterState ms;
    private float hitStopTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        ms = GetComponent<MonsterState>();
        if (ms) ms.OnDamaged += OnDamaged;
    }

    void OnDisable()
    {
        if (ms) ms.OnDamaged -= OnDamaged;
    }

    void OnDamaged(float dmg)
    {
        hitStopTimer = Mathf.Max(hitStopTimer, hitStopSeconds);
    }

    void Start()
    {
        TryWarpToNavMesh();
    }

    void Update()
    {
        if (state == State.Dead) return;

        if (hitStopTimer > 0f || IsHitPlaying())
        {
            hitStopTimer -= Time.deltaTime;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            animator.SetBool("IsMoving", false);
            return;
        }

        float speed = new Vector3(agent.velocity.x, 0, agent.velocity.z).magnitude;
        bool isMoving = agent.hasPath && agent.remainingDistance > agent.stoppingDistance && speed > moveThreshold;
        animator.SetBool("IsMoving", isMoving);
    }

    public void ApplyRemoteState(S_MONSTER_STATE pkt)
    {
        switch (pkt.state)
        {
            case EMonsterState.IDLE:
                agent.isStopped = true;
                break;

            case EMonsterState.WANDER:
                agent.isStopped = false;
                agent.speed = wanderSpeed;
                Vector3 dest = new Vector3(pkt.destX, pkt.destY, pkt.destZ);
                if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 20f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
                break;

            case EMonsterState.CHASE:
                {
                    agent.isStopped = false;
                    agent.speed = chaseSpeed;

                    agent.stoppingDistance = 2f;

                    if (NetworkManager.Instance._players.TryGetValue(pkt.targetId, out GameObject targetUser))
                    {
                        agent.SetDestination(targetUser.transform.position);
                    }
                    break;
                }

            case EMonsterState.ATTACK:
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                    agent.ResetPath();

                    if (pkt.targetId != -1 && NetworkManager.Instance._players.TryGetValue(pkt.targetId, out GameObject targetUser))
                    {
                        Vector3 lookDir = targetUser.transform.position - transform.position;
                        lookDir.y = 0f;
                        if (lookDir.sqrMagnitude > 0.0001f)
                            transform.rotation = Quaternion.LookRotation(lookDir);
                    }

                    animator.SetTrigger(UnityEngine.Random.value > 0.5f ? attackTrigger1 : attackTrigger2);
                    break;
                }

            case EMonsterState.DEAD:
                agent.isStopped = true;
                state = State.Dead;
                break;
        }
    }

    bool IsHitPlaying()
    {
        if (!animator) return false;
        var st = animator.GetCurrentAnimatorStateInfo(0);
        foreach (var tag in hitTags)
            if (!string.IsNullOrEmpty(tag) && st.IsTag(tag)) return st.normalizedTime < 0.98f;
        if (st.IsName("GetHit")) return st.normalizedTime < 0.98f;
        return false;
    }

    void TryWarpToNavMesh()
    {
        if (agent && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
    }
}