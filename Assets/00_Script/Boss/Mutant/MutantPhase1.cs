using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MutantPhase1 : MonoBehaviour
{
    public MutantAI mutantAI;
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

    [Header("사운드")]
    public AudioClip roarSfx;
    public float roarVolume = 1f;
    private AudioSource roarSrc;

    void Awake()
    {
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mutantAI.agent.updateRotation = true;
        mutantAI.agent.isStopped = true;

        roarSrc = gameObject.AddComponent<AudioSource>();
        roarSrc.playOnAwake = false;
        roarSrc.loop = false;
        roarSrc.spatialBlend = 0f;   
        roarSrc.volume = roarVolume;

        StartCoroutine(BeginAfterDelay());
    }

    IEnumerator BeginAfterDelay() {
        roarSrc.PlayOneShot(roarSfx, roarVolume);
        yield return new WaitForSeconds(4f);
        started = true;
    }

    void Update()
    {
        if (!started || !player) return;
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Approach:
                // 탐지 범위 밖이면 대기
                if (dist > detectionRange) { mutantAI.agent.isStopped = true; mutantAI.animator.SetBool("IsWalking", false); mutantAI.animator.SetBool("SlowWalk", false); return; }

                // 공격 사정권이면 공격 시도
                if (dist <= closeStopDistance && attackTimer <= 0f && !IsAttackPlaying())
                {
                    StartAttack();
                    return;
                }

                // 이동
                mutantAI.agent.isStopped = false;
                mutantAI.agent.SetDestination(player.position);

                if (dist > stopDistance)
                {
                    mutantAI.agent.speed = walkSpeed;
                    mutantAI.agent.stoppingDistance = stopDistance;
                    mutantAI.animator.SetBool("IsWalking", true);
                    mutantAI.animator.SetBool("SlowWalk", false);
                }
                else if (dist > closeStopDistance)
                {
                    mutantAI.agent.speed = slowWalkSpeed;
                    mutantAI.agent.stoppingDistance = closeStopDistance;
                    mutantAI.animator.SetBool("IsWalking", false);
                    mutantAI.animator.SetBool("SlowWalk", true);
                }
                else
                {
                    // 사정권인데 쿨타임 중이면 제자리 유지
                    mutantAI.agent.isStopped = true;
                    mutantAI.animator.SetBool("IsWalking", false);
                    mutantAI.animator.SetBool("SlowWalk", false);
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

        mutantAI.agent.isStopped = true;
        mutantAI.agent.ResetPath();
        mutantAI.animator.SetBool("IsWalking", false);
        mutantAI.animator.SetBool("SlowWalk", false);

        int i = Random.Range(0, attackTriggers.Length);
        mutantAI.animator.SetTrigger(attackTriggers[i]);

        attackTimer = attackCooldown;
        state = State.Attack;
    }

    bool IsAttackPlaying()
    {
        var st = mutantAI.animator.GetCurrentAnimatorStateInfo(0);
        if (mutantAI.animator.IsInTransition(0)) return true;
        return st.IsTag("Attack") && st.normalizedTime < 0.98f;
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position; dir.y = 0;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 7f);
    }
}
