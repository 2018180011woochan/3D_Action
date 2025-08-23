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
    public float attackHoldSeconds = 4f;       

    enum State { Chase, Attack }
    State state = State.Chase;

    Transform player;
    bool started = false;

    // 공격 상태 유지/락 관련
    float attackStateEndTime = 0f;
    bool agentWasEnabled;
    bool agentLocked = false;
    Vector3 lockPos;

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
        ActivatePhase2Fire();
        //yield return new WaitForSeconds(startDelay);
        started = true;
        state = State.Chase;
        ResumeChase();
        yield return null;
    }

    [SerializeField] string phase2FireTag = "Phase2Fire";
    [SerializeField] Transform phase2FireRoot;

    Transform ResolvePhase2FireRoot()
    {
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (!t) continue;
            if (!t.gameObject.scene.IsValid() || !t.gameObject.scene.isLoaded) continue; // 프리팹/에셋 제외
            if (t.CompareTag(phase2FireTag))
            {
                phase2FireRoot = t;
                break;
            }
        }
        return phase2FireRoot;
    }

    void ActivatePhase2Fire()
    {
        var root = ResolvePhase2FireRoot();
        if (!root)
        {
            Debug.LogWarning("Phase2 오브젝트를 찾지 못했습니다.");
            return;
        }

        if (!root.gameObject.activeSelf)
            root.gameObject.SetActive(true);

        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t == root) continue;
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);

            var ps = t.GetComponent<ParticleSystem>();
            if (ps) { ps.Clear(true); ps.Play(true); }

            var light = t.GetComponent<Light>();
            if (light) light.enabled = true;
        }
    }
    void Update()
    {
        if (!started || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Chase:
                if (dist <= closeStopDistance && !IsAttackPlaying())
                {
                    StartAttack();
                    return;
                }

                if (mutantAI.agent && mutantAI.agent.enabled)
                {
                    ResumeChase();
                    mutantAI.agent.SetDestination(player.position);
                }
                break;

            case State.Attack:
                if (agentLocked) transform.position = lockPos;

                FacePlayer();

                if (Time.time >= attackStateEndTime)
                {
                    if (dist > closeStopDistance)
                    {
                        ReenableAgentAndChase();
                    }
                    else
                    {
                        StartAttack(); 
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

        attackStateEndTime = Time.time + attackHoldSeconds;

        if (mutantAI.agent)
        {
            agentWasEnabled = mutantAI.agent.enabled;
            lockPos = transform.position;
            mutantAI.agent.enabled = false;
            agentLocked = true;
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
