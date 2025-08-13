using System.Collections;
using UnityEngine;

public class MutantPhase2 : MonoBehaviour
{
    public MutantAI mutantAI;
    public float closeStopDistance = 2f;
    public float runSpeed = 5.5f;
    public float rotationSpeed = 10f;
    public string runBool = "IsRunning";
    Transform player;
    bool started = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mutantAI.agent.updateRotation = true;
        mutantAI.agent.isStopped = true;

        if (mutantAI.animator) mutantAI.animator.applyRootMotion = false;
        if (mutantAI.agent)
        {
            mutantAI.agent.updateRotation = true;
            mutantAI.agent.isStopped = true;     // 대기 시작
            mutantAI.agent.stoppingDistance = closeStopDistance;
        }

        // 달리기 파라미터 초기화
        if (mutantAI.animator)
        {
            mutantAI.animator.SetBool("IsWalking", false);
            mutantAI.animator.SetBool("SlowWalk", false);
            mutantAI.animator.SetBool(runBool, false);
        }

        StartCoroutine(BeginAfterDelay());
    }

    IEnumerator BeginAfterDelay() { yield return new WaitForSeconds(4f); started = true; }

    void Update()
    {
        if (!started || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > closeStopDistance)
        {
            // 전력질주 추격
            var ag = mutantAI.agent;
            if (ag)
            {
                ag.isStopped = false;
                ag.speed = runSpeed;
                ag.stoppingDistance = closeStopDistance;
                ag.SetDestination(player.position);
            }

            if (mutantAI.animator)
            {
                mutantAI.animator.SetBool("IsWalking", false);
                mutantAI.animator.SetBool("SlowWalk", false);
                mutantAI.animator.SetBool(runBool, true); // Run 애니메이션 ON
            }
        }
        else
        {
            // 근접 도달 → 정지 + 플레이어 바라보기
            var ag = mutantAI.agent;
            if (ag)
            {
                ag.isStopped = true;
                ag.velocity = Vector3.zero;
                ag.ResetPath();
                ag.nextPosition = transform.position;
            }

            if (mutantAI.animator) mutantAI.animator.SetBool(runBool, false);
            FacePlayer();
        }
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            var targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }
}
