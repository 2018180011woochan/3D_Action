using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MutantPhase1 : MonoBehaviour
{
    public MutantAI ｍutantAI;
    public float detectionRange = 30f;
    public float stopDistance = 6f;       // 슬로우 걷기 시작 거리
    public float closeStopDistance = 2f;  // 근접(공격 거리)
    public float walkSpeed = 2.5f;
    public float slowWalkSpeed = 1.2f;

    public string[] attackTriggers = { "Attack1", "Attack2", "Attack3", "Attack4" };
    public float attackCooldown = 2.5f;

    enum State { Approach, Attack }
    State state = State.Approach;

    Transform player;
    bool started = false;
    float attackTimer = 0f;

    void Awake()
    {
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ｍutantAI.agent.updateRotation = true;
        ｍutantAI.agent.isStopped = true;
        StartCoroutine(BeginAfterDelay());
    }

    IEnumerator BeginAfterDelay() { yield return new WaitForSeconds(4f); started = true; }

    void Update()
    {
        if (!started || !player) return;
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Approach:
                // 탐지 범위 밖이면 대기
                if (dist > detectionRange) { ｍutantAI.agent.isStopped = true; ｍutantAI.animator.SetBool("IsWalking", false); ｍutantAI.animator.SetBool("SlowWalk", false); return; }

                // 공격 사정권이면 공격 시도
                if (dist <= closeStopDistance && attackTimer <= 0f && !IsAttackPlaying())
                {
                    StartAttack();
                    return;
                }

                // 이동
                ｍutantAI.agent.isStopped = false;
                ｍutantAI.agent.SetDestination(player.position);

                if (dist > stopDistance)
                {
                    ｍutantAI.agent.speed = walkSpeed;
                    ｍutantAI.agent.stoppingDistance = stopDistance;
                    ｍutantAI.animator.SetBool("IsWalking", true);
                    ｍutantAI.animator.SetBool("SlowWalk", false);
                }
                else if (dist > closeStopDistance)
                {
                    ｍutantAI.agent.speed = slowWalkSpeed;
                    ｍutantAI.agent.stoppingDistance = closeStopDistance;
                    ｍutantAI.animator.SetBool("IsWalking", false);
                    ｍutantAI.animator.SetBool("SlowWalk", true);
                }
                else
                {
                    // 사정권인데 쿨타임 중이면 제자리 유지
                    ｍutantAI.agent.isStopped = true;
                    ｍutantAI.animator.SetBool("IsWalking", false);
                    ｍutantAI.animator.SetBool("SlowWalk", false);
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

        ｍutantAI.agent.isStopped = true;
        ｍutantAI.agent.ResetPath();
        ｍutantAI.animator.SetBool("IsWalking", false);
        ｍutantAI.animator.SetBool("SlowWalk", false);

        int i = Random.Range(0, attackTriggers.Length);
        ｍutantAI.animator.SetTrigger(attackTriggers[i]);

        attackTimer = attackCooldown;
        state = State.Attack;
    }

    bool IsAttackPlaying()
    {
        var st = ｍutantAI.animator.GetCurrentAnimatorStateInfo(0);
        if (ｍutantAI.animator.IsInTransition(0)) return true;
        return st.IsTag("Attack") && st.normalizedTime < 0.98f;
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 7f);
    }
}
