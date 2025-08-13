using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MutantPhase2 : MonoBehaviour
{
    public MutantAI mutantAI;

    [Header("Start")]
    public float startDelay = 4f;

    [Header("Move / Chase")]
    public float runSpeed = 6.5f;
    public float closeStopDistance = 2f;
    public string runBool = "IsRunning";

    [Header("Attack")]
    public string[] attackTriggers = { "Attack1", "Attack2", "Attack3", "Attack4" };
    public float attackCooldown = 2.2f;

    [Header("Agent Lock During Attack")]
    public float agentReenableDelay = 2.5f; // ← 요구한 2.5초
    float agentLockUntil = 0f;
    bool agentWasEnabled;
    bool agentLocked = false;
    Vector3 lockPos;

    enum State { Chase, Attack }
    State state = State.Chase;

    Transform player;
    bool started = false;
    float attackTimer = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (mutantAI.animator) mutantAI.animator.applyRootMotion = false;
        if (mutantAI.agent)
        {
            mutantAI.agent.updateRotation = true;
            mutantAI.agent.isStopped = true;
            mutantAI.agent.stoppingDistance = closeStopDistance;
        }

        if (mutantAI.animator)
        {
            mutantAI.animator.SetBool("IsWalking", false);
            mutantAI.animator.SetBool("SlowWalk", false);
            mutantAI.animator.SetBool(runBool, false);
        }

        StartCoroutine(BeginAfterDelay());
    }

    IEnumerator BeginAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
        started = true;
    }

    void Update()
    {
        if (!started || !player) return;

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Chase:
                // 사정권 & 쿨타임 끝 → 공격 시작
                if (dist <= closeStopDistance && attackTimer <= 0f && !IsAttackPlaying())
                {
                    StartAttack();
                    return;
                }

                // 사정권인데 쿨타임 중 → 멈추고 응시
                if (dist <= closeStopDistance && attackTimer > 0f)
                {
                    StopAgent();
                    if (mutantAI.animator) mutantAI.animator.SetBool(runBool, false);
                    FacePlayer();
                    return;
                }

                // 추격
                if (mutantAI.agent && mutantAI.agent.enabled)
                {
                    ResumeChase();
                    mutantAI.agent.SetDestination(player.position);
                }
                break;

            case State.Attack:
                // 공격 동안은 제자리 고정(혹시 모를 미세 이동 봉쇄)
                if (agentLocked)
                    transform.position = lockPos;

                FacePlayer();

                // 2.5초 잠금 해제 시점에만 "추격 필요" 판단 → 필요하면 에이전트 재활성화 + Chase 전환
                if (Time.time >= agentLockUntil)
                {
                    if (dist > closeStopDistance)
                    {
                        ReenableAgentAndChase();
                        return;
                    }
                    // 사정권이면 에이전트는 계속 OFF 상태 유지(제자리에서 추가 공격 가능)
                }

                // 공격 애니가 끝났을 때의 처리
                if (!IsAttackPlaying())
                {
                    // 사정권 & 쿨타임 끝 → 연속 공격 (여기서도 에이전트는 그대로 OFF 유지)
                    if (dist <= closeStopDistance && attackTimer <= 0f)
                    {
                        StartAttack(); // 새 공격 시작하면 lock 타이머도 새로 설정됨
                    }
                    else
                    {
                        // 사정권 밖인데 아직 2.5초가 안 지났으면 그대로 대기(Agent OFF 유지)
                        // 아무 것도 하지 않음. (요구사항대로 2.5초 지나야만 키고 추격)
                    }
                }
                break;
        }
    }

    void StartAttack()
    {
        if (attackTriggers == null || attackTriggers.Length == 0) return;

        StopAgent();
        if (mutantAI.animator) mutantAI.animator.SetBool(runBool, false);

        int i = Random.Range(0, attackTriggers.Length);
        mutantAI.animator.SetTrigger(attackTriggers[i]);

        attackTimer = attackCooldown;

        if (mutantAI.agent)
        {
            agentWasEnabled = mutantAI.agent.enabled;
            lockPos = transform.position;
            mutantAI.agent.enabled = false; 
            agentLocked = true;
            agentLockUntil = Time.time + agentReenableDelay;
        }

        state = State.Attack;
    }

    void ReenableAgentAndChase()
    {
        if (mutantAI.agent)
        {
            mutantAI.agent.enabled = agentWasEnabled;     
            mutantAI.agent.Warp(transform.position);      
        }

        agentLocked = false; 
        state = State.Chase;

        ResumeChase();
        if (mutantAI.agent && mutantAI.agent.enabled)
            mutantAI.agent.SetDestination(player.position);
    }

    void StopAgent()
    {
        if (!mutantAI.agent || !mutantAI.agent.enabled) return;
        mutantAI.agent.isStopped = true;
        mutantAI.agent.ResetPath();
        mutantAI.agent.velocity = Vector3.zero;
    }

    void ResumeChase()
    {
        if (!mutantAI.agent || !mutantAI.agent.enabled) return;
        mutantAI.agent.isStopped = false;
        mutantAI.agent.speed = runSpeed;
        mutantAI.agent.stoppingDistance = closeStopDistance;
        if (mutantAI.animator) mutantAI.animator.SetBool(runBool, true);
    }

    bool IsAttackPlaying()
    {
        if (!mutantAI.animator) return false;
        var st = mutantAI.animator.GetCurrentAnimatorStateInfo(0);
        if (mutantAI.animator.IsInTransition(0)) return true;
        return st.IsTag("Attack") && st.normalizedTime < 0.98f;
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
    }
}
