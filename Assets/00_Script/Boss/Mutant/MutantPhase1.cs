using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MutantPhase1 : MonoBehaviour
{
    public float detectionRange = 30f;
    public float stopDistance = 6f;       // 슬로우 걷기 시작 거리
    public float closeStopDistance = 2f;  // 근접(공격 거리)
    public float walkSpeed = 2.5f;
    public float slowWalkSpeed = 1.2f;

    public string[] attackTriggers = { "Attack1", "Attack2", "Attack3", "Attack4" };
    public float attackCooldown = 2.5f;

    enum State { Approach, Attack }
    State state = State.Approach;

    NavMeshAgent agent;
    Animator animator;
    Transform player;
    bool started = false;
    float attackTimer = 0f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.updateRotation = true;
        agent.isStopped = true;
        StartCoroutine(BeginAfterDelay());
    }

    IEnumerator BeginAfterDelay() { yield return new WaitForSeconds(2f); started = true; }

    void Update()
    {
        if (!started || !player) return;
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Approach:
                // 탐지 범위 밖이면 대기
                if (dist > detectionRange) { agent.isStopped = true; animator.SetBool("IsWalking", false); animator.SetBool("SlowWalk", false); return; }

                // 공격 사정권이면 공격 시도
                if (dist <= closeStopDistance && attackTimer <= 0f && !IsAttackPlaying())
                {
                    StartAttack();
                    return;
                }

                // 이동
                agent.isStopped = false;
                agent.SetDestination(player.position);

                if (dist > stopDistance)
                {
                    agent.speed = walkSpeed;
                    agent.stoppingDistance = stopDistance;
                    animator.SetBool("IsWalking", true);
                    animator.SetBool("SlowWalk", false);
                }
                else if (dist > closeStopDistance)
                {
                    agent.speed = slowWalkSpeed;
                    agent.stoppingDistance = closeStopDistance;
                    animator.SetBool("IsWalking", false);
                    animator.SetBool("SlowWalk", true);
                }
                else
                {
                    // 사정권인데 쿨타임 중이면 제자리 유지
                    agent.isStopped = true;
                    animator.SetBool("IsWalking", false);
                    animator.SetBool("SlowWalk", false);
                    FacePlayer();
                }
                break;

            case State.Attack:
                FacePlayer();
                if (!IsAttackPlaying())
                {
                    // 연속공격 조건
                    if (dist <= closeStopDistance && attackTimer <= 0f)
                        StartAttack();
                    else
                        state = State.Approach; // 다시 추격(또는 대치)
                }
                break;
        }
    }

    void StartAttack()
    {
        if (attackTriggers == null || attackTriggers.Length == 0) return;

        agent.isStopped = true;
        agent.ResetPath();
        animator.SetBool("IsWalking", false);
        animator.SetBool("SlowWalk", false);

        int i = Random.Range(0, attackTriggers.Length);
        animator.SetTrigger(attackTriggers[i]);

        attackTimer = attackCooldown;
        state = State.Attack;
    }

    bool IsAttackPlaying()
    {
        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (animator.IsInTransition(0)) return true;
        return st.IsTag("Attack") && st.normalizedTime < 0.98f;
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 7f);
    }
}
