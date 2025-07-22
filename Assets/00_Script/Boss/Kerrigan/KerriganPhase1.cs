using UnityEngine;

public class KerriganPhase1 : MonoBehaviour
{
    public enum State
    {
        Chase,
        Confront,
        KickAttack,
        RangedAttack,
        MeleeCombo,
        BackStep
    }

    private State currentState = State.Chase;

    [Header("대치 설정")]
    public float confrontDecisionTime = 3f;
    private float confrontTimer = 0f;

    [Header("킥 공격 설정")]
    public float kickRange = 4f;
    public float kickDuration = 3f;
    public float kickApproachTimeout = 5f;
    public GameObject kickEffectPrefab;
    private float kickTimer = 0f;
    private bool isApproachingForKick = true;  // 킥을 위해 접근 중인지
    private bool isExecutingKick = false;

    [Header("원거리 공격 설정")]
    public float rangedAttackDuration = 2f;
    public float projectileSpawnTime = 1.2f;
    public GameObject projectilePrefab;
    private float rangedTimer = 0f;
    private bool hasSpawnedProjectile = false;

    [Header("근접 콤보 설정")]
    public float leftHookDuration = 1.15f;
    public float leftHookMoveSpeed = 2f;
    public float swingDuration = 0.5f;
    public float swingForwardDistance = 3f;


    [Header("백스텝 설정")]
    public float backStepDistance = 5f;
    public float backStepSpeed = 2f;
    public float backStepDuration = 1.5f;
    private float backStepTimer = 0f;
    private State stateBeforeBackStep;

    // 근접 콤보 상태 변수
    private enum MeleePhase { LeftHook, Swing }
    private MeleePhase currentMeleePhase = MeleePhase.LeftHook;
    private float meleeTimer = 0f;
    private Vector3 meleeDirection;
    private Vector3 swingStartPos;

    // 메인 AI 참조
    private BossAI BossAI;

    void Awake()
    {
        BossAI = GetComponent<BossAI>();
    }

    void OnEnable()
    {
        ChangeState(State.Chase);
    }

    void Update()
    {
        if (BossAI.IsHit())
        {
            return;
        }

        switch (currentState)
        {
            case State.Chase:
                UpdateChase();
                break;
            case State.Confront:
                UpdateConfront();
                break;
            case State.KickAttack:
                UpdateKickAttack();
                break;
            case State.RangedAttack:
                UpdateRangedAttack();
                break;
            case State.MeleeCombo:
                UpdateMeleeCombo();
                break;
            case State.BackStep:  
                UpdateBackStep();
                break;
        }
    }

    void UpdateChase()
    {
        float distance = BossAI.GetDistanceToPlayer();

        // 추격
        BossAI.Agent.SetDestination(BossAI.Player.position);
        BossAI.Agent.isStopped = false;
        BossAI.Agent.speed = BossAI.walkSpeed;

        // 애니메이션
        BossAI.Animator.SetBool("isWalking", true);

        // 가까워지면 대치로 전환
        if (distance < BossAI.farDistance)
        {
            ChangeState(State.Confront);
        }
    }

    void UpdateConfront()
    {
        float distance = BossAI.GetDistanceToPlayer();

        // 거리별 대응
        if (distance > BossAI.veryFarDistance)
        {
            ChangeState(State.Chase);
            return;
        }
        else if (distance < BossAI.closeDistance)
        {
            if (Random.Range(0, 2) == 0)
            {
                // 백스텝
                stateBeforeBackStep = State.Confront;
                ChangeState(State.BackStep);
            }
            else
            {
                // 근접 콤보
                ChangeState(State.MeleeCombo);
            }
            return;
        }

        // 공전
        OrbitAroundPlayer();

        // 타이머
        confrontTimer += Time.deltaTime;
        if (confrontTimer >= confrontDecisionTime)
        {
            confrontTimer = 0f;
            DecideNextAction();
        }
    }

    void UpdateBackStep()
    {
        backStepTimer += Time.deltaTime;

        // 뒤로 물러나기
        Vector3 backDirection = (transform.position - BossAI.Player.position).normalized;
        transform.position += backDirection * backStepSpeed * Time.deltaTime;

        // 백스텝 완료
        if (backStepTimer >= backStepDuration)
        {
            ChangeState(State.Confront);
        }
    }

    void OrbitAroundPlayer()
    {
        Vector3 direction = (BossAI.Player.position - transform.position).normalized;
        Vector3 orbitDir = Vector3.Cross(Vector3.up, direction);
        transform.position += orbitDir * BossAI.confrontSpeed * Time.deltaTime;

        BossAI.Animator.SetBool("isMovingRight", true);
    }

    void DecideNextAction()
    {
        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                // 계속 공전
                Debug.Log("Phase1: 계속 공전");
                break;
            case 1:
                ChangeState(State.KickAttack);
                break;
            case 2:
                ChangeState(State.RangedAttack);
                break;
        }
    }

    void UpdateKickAttack()
    {
        kickTimer += Time.deltaTime;
        float distance = BossAI.GetDistanceToPlayer();

        // 1단계: 플레이어에게 접근
        if (isApproachingForKick && !isExecutingKick)
        {
            // 플레이어 추격
            BossAI.Agent.SetDestination(BossAI.Player.position);
            BossAI.Agent.isStopped = false;

            // 애니메이션 - 달리기
            BossAI.Animator.SetBool("isRunning", true);
            BossAI.Animator.SetBool("isWalking", false);

            // 사거리 내에 들어왔는지 체크
            if (distance <= kickRange)
            {
                // 킥 실행 단계로 전환
                isApproachingForKick = false;
                isExecutingKick = true;
                kickTimer = 0f;

                // 에이전트 정지
                BossAI.Agent.isStopped = true;
                BossAI.Agent.velocity = Vector3.zero;

                // 킥 애니메이션 트리거
                BossAI.Animator.SetTrigger("Kick");
                BossAI.Animator.SetBool("isRunning", false);

                Debug.Log("Phase1: 킥 실행!");
            }
            // 시간 초과 체크
            else if (kickTimer >= kickApproachTimeout)
            {
                Debug.Log("Phase1: 킥 공격 포기 - 플레이어가 너무 멀어짐");
                ChangeState(State.Confront);
            }
        }
        // 2단계: 킥 실행
        else if (isExecutingKick)
        {
            // 킥 애니메이션이 끝날 때까지 대기
            if (kickTimer >= kickDuration)
            {
                // 킥 완료, 대치 상태로 복귀
                ChangeState(State.Confront);
            }
        }
    }

    void UpdateRangedAttack()
    {
        rangedTimer += Time.deltaTime;

        // 투사체 생성
        if (!hasSpawnedProjectile && rangedTimer >= projectileSpawnTime)
        {
            SpawnProjectile();
            hasSpawnedProjectile = true;
        }

        if (rangedTimer >= rangedAttackDuration)
        {
            ChangeState(State.Confront);
        }
    }

    void UpdateMeleeCombo()
    {
        meleeTimer += Time.deltaTime;

        switch (currentMeleePhase)
        {
            case MeleePhase.LeftHook:
                HandleLeftHook();
                break;
            case MeleePhase.Swing:
                HandleSwing();
                break;
        }
    }

    void HandleLeftHook()
    {
        // 전진하며 공격
        transform.position += meleeDirection * leftHookMoveSpeed * Time.deltaTime;

        if (meleeTimer >= leftHookDuration)
        {
            currentMeleePhase = MeleePhase.Swing;
            meleeTimer = 0f;
            swingStartPos = transform.position;
            meleeDirection = (BossAI.Player.position - transform.position).normalized;
            meleeDirection.y = 0;
            BossAI.Animator.SetTrigger("Swing");
            Debug.Log("Phase1: Swing 시작");
        }
    }

    void HandleSwing()
    {
        // 전진하며 스윙
        float t = Mathf.Min(meleeTimer / swingDuration, 1f);
        transform.position = swingStartPos + meleeDirection * (swingForwardDistance * t);

        if (meleeTimer >= swingDuration)
        {
            stateBeforeBackStep = State.MeleeCombo;
            ChangeState(State.BackStep);
        }
    }

    void SpawnProjectile()
    {
        if (projectilePrefab != null)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1f + Vector3.up * 3f;
            Instantiate(projectilePrefab, spawnPos, transform.rotation);
        }
    }

    // 상태 변경
    void ChangeState(State newState)
    {
        // 이전 상태 종료
        ExitState(currentState);

        // 새 상태로 변경
        currentState = newState;
        Debug.Log($"Phase1 상태 변경: {newState}");

        // 새 상태 시작
        EnterState(newState);
    }

    void EnterState(State state)
    {
        switch (state)
        {
            case State.Chase:
                BossAI.Agent.isStopped = false;
                break;

            case State.Confront:
                BossAI.Agent.isStopped = true;
                confrontTimer = 0f;
                break;

            case State.KickAttack:
                kickTimer = 0f;
                isApproachingForKick = true;
                isExecutingKick = false;

                BossAI.Agent.isStopped = false;
                BossAI.Agent.speed = BossAI.runSpeed;
                BossAI.Agent.stoppingDistance = kickRange - 0.5f; // 여유 거리

                // 킥 시작 이펙트
                if (kickEffectPrefab != null)
                    Instantiate(kickEffectPrefab, transform.position, transform.rotation);

                Debug.Log("Phase1: 킥 공격 시작 - 접근 중");
                break;

            case State.RangedAttack:
                rangedTimer = 0f;
                hasSpawnedProjectile = false;
                BossAI.Agent.isStopped = true;
                BossAI.Animator.SetTrigger("RangeAttack");
                break;

            case State.MeleeCombo:
                meleeTimer = 0f;
                currentMeleePhase = MeleePhase.LeftHook;
                meleeDirection = (BossAI.Player.position - transform.position).normalized;
                meleeDirection.y = 0;
                BossAI.Animator.SetTrigger("LeftHook");
                Debug.Log("Phase1: 근접 콤보 시작 - LeftHook");
                break;
            case State.BackStep:
                backStepTimer = 0f;
                BossAI.Agent.isStopped = true;
                BossAI.Animator.SetBool("isWalkingBack", true);
                Debug.Log($"백스텝 시작! (이전 상태: {stateBeforeBackStep})");
                break;
        }
    }

    void ExitState(State state)
    {
        // 상태별 종료 처리
        switch (state)
        {
            case State.Chase:
                BossAI.Animator.SetBool("isWalking", false);
                break;
            case State.Confront:
                BossAI.Animator.SetBool("isMovingRight", false);
                BossAI.Animator.SetBool("isWalkingBack", false);
                break;
            case State.KickAttack:  // 추가
                BossAI.Animator.SetBool("isRunning", false);
                BossAI.Agent.stoppingDistance = BossAI.closeDistance; // 원래 값으로 복구
                break;
            case State.MeleeCombo:  // 추가
                BossAI.Animator.SetBool("isWalkingBack", false);
                break;
            case State.BackStep:
                BossAI.Animator.SetBool("isWalkingBack", false);
                break;
        }
    }

    public void OnPhaseExit()
    {
        ExitState(currentState);
        BossAI.Agent.isStopped = true;
    }
}