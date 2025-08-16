using System.Collections;
using System.Collections.Generic;
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

    [Header("공격 공통")]
    public string[] attackTriggers = { "Attack1", "Attack2", "Attack3" };
    public float attackStateDuration = 5f;

    [Header("Attack1")]
    public GameObject attack1Prefab;
    public Transform projectileOrigin;
    public float spawnDistance = 1f;

    [Header("Attack2 (격자 스폰)")]
    public GameObject attack2Prefab;
    public int gridSize = 9;                   // 9×9
    public float gridSpacing = 6f;             // 칸 간격
    public int cellsToSpawn = 40;              // 소환할 칸 수
    public bool alignToGround = true;          // 지면에 붙이기
    public float groundRayUp = 10f;            // 레이 시작 높이
    public float groundRayDown = 50f;          // 레이 아래 길이
    public float groundYOffset = 0.01f;        // 약간 띄우기
    public LayerMask groundMask = ~0;          // 지면 레이어

    public GameObject attack3Prefab;

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
        {
            SpawnAttack1();
        }
        else if (trigger == "Attack2")
        {
            SpawnAttack2();
        }
        else if (trigger == "Attack3") {
            SpawnAttack3();
        }

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

    public void SpawnAttack2()
    {
        if (!attack2Prefab) return;

        int size = Mathf.Max(1, gridSize);       
        if (size % 2 == 0) size += 1;           
        int half = size / 2;

        Vector3 center = transform.position;
        Vector3 right = transform.right;
        Vector3 fwd = transform.forward;

        var cells = new List<Vector3>(size * size);
        for (int z = -half; z <= half; z++)
        {
            for (int x = -half; x <= half; x++)
            {
                Vector3 pos = center
                              + right * (x * gridSpacing)
                              + fwd * (z * gridSpacing);

                if (alignToGround && TrySnapToGround(pos, out Vector3 groundPos))
                    pos = groundPos + Vector3.up * groundYOffset;

                cells.Add(pos);
            }
        }

        int toSpawn = Mathf.Clamp(cellsToSpawn, 0, cells.Count);
        for (int i = 0; i < toSpawn; i++)
        {
            int k = Random.Range(i, cells.Count);
            (cells[i], cells[k]) = (cells[k], cells[i]);
        }

        Quaternion rot = transform.rotation; 
        for (int i = 0; i < toSpawn; i++)
            Instantiate(attack2Prefab, cells[i], rot);
    }

    public void SpawnAttack3()
    {
        if (!attack3Prefab) return;

        Vector3 center = transform.position;
        Quaternion rot = transform.rotation;

        // 첫 번째
        var a = Instantiate(attack3Prefab, center, rot);
        var swa = a.GetComponent<RotateSectorWarning>();
        float r = 6f;
        if (swa)
        {
            swa.orbitCenter = transform;
            swa.fillDuration = 3f;
            if (swa.orbitRadius > 0f) r = swa.orbitRadius; // 프리팹 설정값 사용
        }
        a.transform.position = center + transform.forward * r;

        // 반대편 두 번째
        var b = Instantiate(attack3Prefab, center, rot);
        var swb = b.GetComponent<RotateSectorWarning>();
        if (swb)
        {
            swb.orbitCenter = transform;
            swb.fillDuration = 3f;
            // swb.orbitRadius = r; // (같게 강제하고 싶으면 활성화)
        }
        b.transform.position = center - transform.forward * r;
    }
    bool TrySnapToGround(Vector3 pos, out Vector3 snapped)
    {
        Vector3 start = pos + Vector3.up * groundRayUp;
        float dist = groundRayUp + groundRayDown;
        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, dist, groundMask, QueryTriggerInteraction.Ignore))
        {
            snapped = hit.point;
            return true;
        }
        snapped = pos;
        return false;
    }
}
