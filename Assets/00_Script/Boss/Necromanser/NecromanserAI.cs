using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NecromanserAI : MonoBehaviour
{
    [Header("탐지/거리")]
    public float detectionRange = 30f;
    public float attackStartDistance = 10f;   
    public float disengageRange = 40f;         

    [Header("이동")]
    public float walkSpeed = 2.5f;
    public string walkBool = "IsWalking";

    [Header("공격")]
    public string[] attackTriggers = { "Attack1", "Attack2", "Attack3" };
    public float attackStateDuration = 5f;

    [Header("Attack1")]
    public GameObject attack1Prefab;     
    public Transform projectileOrigin;  
    public float spawnDistance = 1f;    // 보스 중심에서 얼마나 떨어뜨려 생성할지

    enum State { Chase, Attack }
    State state = State.Chase;

    Transform player;
    bool inAttackRoutine;
    Coroutine attackRoutine;
    int attackIndex = 0;                       

    public NavMeshAgent agent;
    public Animator animator;
    private MonsterState monsterState;

    void Awake()
    {
        monsterState = GetComponent<MonsterState>();
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent.stoppingDistance = attackStartDistance;
        agent.updateRotation = true;
        agent.isStopped = true;
        SetWalk(false);
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Chase:
                if (dist > detectionRange)
                {
                    agent.isStopped = true;
                    SetWalk(false);
                    return;
                }

                if (dist <= attackStartDistance && !inAttackRoutine)
                {
                    EnterAttackState();
                    return;
                }

                agent.isStopped = false;
                agent.speed = walkSpeed;
                agent.SetDestination(player.position);
                SetWalk(true);
                break;

            case State.Attack:
                if (dist > disengageRange && inAttackRoutine)
                {
                    ExitAttackState();
                    state = State.Chase;
                }
                else
                {
                    FacePlayer();
                }
                break;
        }
    }

    void EnterAttackState()
    {
        state = State.Attack;

        agent.isStopped = true;
        agent.ResetPath();
        agent.updateRotation = false;
        SetWalk(false);

        FireNextAttackInSequence();

        if (!inAttackRoutine)
            attackRoutine = StartCoroutine(AttackHold());
    }

    void ExitAttackState()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        inAttackRoutine = false;

        agent.updateRotation = true;
        agent.isStopped = false;

        state = State.Chase;
    }

    IEnumerator AttackHold()
    {
        inAttackRoutine = true;
        float endTime = Time.time + attackStateDuration;

        while (Time.time < endTime)
        {
            FacePlayer();
            yield return null;
        }

        while (IsAttackPlaying())
            yield return null;

        inAttackRoutine = false;

        agent.updateRotation = true;
        agent.isStopped = false;
        state = State.Chase;
    }

    void FireNextAttackInSequence()
    {
        if (attackTriggers == null || attackTriggers.Length == 0) return;

        foreach (var t in attackTriggers)
            animator.ResetTrigger(t);

        string trigger = attackTriggers[attackIndex % attackTriggers.Length];
        animator.SetTrigger(trigger);

        if (trigger == "Attack1")
            SpawnAttack1();
        else if (trigger == "Attack2")

        attackIndex++; 
    }

    bool IsAttackPlaying()
    {
        if (!animator) return false;
        if (animator.IsInTransition(0)) return true;

        var st = animator.GetCurrentAnimatorStateInfo(0);
        return st.IsTag("Attack") && st.normalizedTime < 0.98f;
    }

    void FacePlayer()
    {
        if (!player) return;
        Vector3 dir = player.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            var targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }

    void SetWalk(bool on)
    {
        if (animator) animator.SetBool(walkBool, on);
    }

    public void SpawnAttack1()
    {
        if (!attack1Prefab) return;

        Transform originT = projectileOrigin ? projectileOrigin : this.transform;
        Vector3 origin = originT.position;

        for (int i = 0; i < 4; i++)
        {
            Quaternion yaw = Quaternion.AngleAxis(90f * i, Vector3.up);
            Vector3 dir = yaw * transform.forward;              
            Vector3 spawnPos = origin + dir.normalized * spawnDistance;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            Instantiate(attack1Prefab, spawnPos, rot);
        }
    }
}
