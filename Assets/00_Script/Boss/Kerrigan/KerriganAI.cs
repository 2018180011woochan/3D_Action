using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class KerriganAI : MonoBehaviour
{
/*    [Header("보스 상태")]
    public float maxHp = 100f;
    public float currentHp;*/

    [Header("거리 설정")]
    public float veryFarDistance = 20f;  
    public float farDistance = 10f; 
    public float closeDistance = 3f;     

    [Header("이동 설정")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("대치 관련")]
    public float confrontSpeed = 1.5f;  // 공전 속도
    public float backStepDistance = 5f;  
    public float backStepSpeed = 2f;     
    private bool isBackStep = false;

    private float confrontTimer = 0f;   // 대치 상태일때 어떤 행동을 할지에 대한 타이머
    public float confrontDecisionTime = 3f;

    [Header("킥 공격 관련")]
    public float kickRange = 4f;              // 킥 공격 사거리
    public float kickDuration = 3f;           // 킥 애니메이션 지속 시간
    private bool isPerformingKick = false;    // 킥 동작 중인지
    private float kickTimer = 0f;             // 킥 타이머
    private bool isMovingToKick = false;      // 킥 위치로 이동 중인지
    public float kickApproachTimeout = 5f;
    public GameObject kickStartEffect;

    [Header("근접 공격 관련")]
    private bool isPerformingMelee = false;    
    private float meleeTimer = 0f;             // 공격 시작한지 몇 초 됐나?
    public float leftHookDuration = 1.15f;      // 레프트훅은 1.15초 걸림
    private bool hasDecidedCloseAction = false;
    public float leftHookMoveSpeed = 2f;        // 추가
    private Vector3 leftHookDirection;

    [Header("스윙 공격 관련")]
    public float swingForwardDistance = 3f;    // 앞으로 이동할 거리
    public float swingDuration = 0.5f;         // 스윙 애니메이션/이동 지속 시간
    private bool isPerformingSwing = false;    // 스윙 중인지
    private float swingTimer = 0f;             // 스윙 타이머
    private Vector3 swingStartPos;
    private Vector3 swingDirection;

    [Header("페이즈1 투사체")]
    public GameObject pase1Projectile;
    public float rangedAttackDuration = 2.08f;  // 원거리 공격 애니메이션 지속 시간
    public float projectileSpawnTime = 1.2f;    // 공격하려고 팔을 뻗는 프레임 시간 
    private bool isPerformingRanged = false;    // 원거리 공격 중인지
    private float rangedTimer = 0f;             // 원거리 공격 타이머
    private bool hasSpawnedProjectile = false;

    [Header("페이즈 2 관련")]
    private bool isPhase2 = false; // 2페이즈 최초 진입했는가
    private float phase2Timer = 0f; // 2페이즈 타이머
    public float flyAltitude = 8f;
    public float flyUpAnimDuration = 5f;

    public float upConfrontSpeed = 4f;  // 공중에서 공전하는 속도
    public float upDuration = 5f; // 공전 지속 시간 
    public float restDuration = 3f;  // 착지 후 휴식 시간 

    [Header("페이즈 2 공격 관련")]
    public int projectileCount = 10;                    // 생성할 투사체 개수
    public float projectileSpawnRadius = 8f;            // 보스 주위 투사체 생성 반경
    public float projectileSpawnInterval = 0.5f;        // 투사체 생성 간격 (1초에 2개)
    public float chargeDuration = 5f;                   // 기 모으는 시간
    public float projectileLaunchRadius = 3f;           // 플레이어 주위 투사체 착탄 반경
    public GameObject phase2Projectile;                 // 2페이즈 투사체 프리팹
    public GameObject groundEffect;                 

    private List<GameObject> spawnedProjectiles = new List<GameObject>();  // 생성된 투사체 리스트
    private float projectileSpawnTimer = 0f;            // 투사체 생성 타이머
    private int currentProjectileCount = 0;              // 현재 생성된 투사체 개수
    private bool isCharging = false;                     // 기 모으는 중인지
    private bool hasStartedLaunch = false;               // 발사 시작했는지

    [Header("페이즈 2 착지 관련")]
    public float flyDownSpeed = 5f;              // 착지 속도
    public float landingDuration = 2f;           // 착지 애니메이션 지속 시간
    public float restingDuration = 3f;           // 휴식 시간
    private bool isLanding = false;              // 착지 중인지
    private bool isResting = false;              // 휴식 중인지
    private Vector3 landingTargetPosition;       // 착지 목표 위치

    private enum ConfrontAction
    {
        Circling,    // 공전
        KickAttack,  // 킥 공격
        RangedAttack // 원거리 공격
    }
    private ConfrontAction currentConfrontAction = ConfrontAction.Circling;

    [Header("참조")]
    public Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    public enum Phase1State
    {
        Walk,           
        Confronting,    // 대치 
        Attack,         
    }
    public Phase1State currentState = Phase1State.Walk;

    public enum Phase2State
    {
        Uping,  // 공중으로 떠오르는 중
        FlyContront,   // 공중에서 공전하는 중
        FlyAttack,  // 원거리 공격 중
        Landing,    // 착지 중
        Resting,     // 휴식 중
        Dead
    }
    public Phase2State currentPhase2State;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = closeDistance;
        }

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;
        if (currentPhase2State == Phase2State.Dead) return;
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit")) return;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (IsPhase1())
            HandlePhase1Behavior(distanceToPlayer);
        else
        {
            HandlePhase2Behavior();
        }
        
        LookAtPlayer();
        UpdateAnimatorParameters();
    }

    void HandlePhase2Behavior()
    {
        // 2페이즈에 처음 진입하는 순간 1회만 실행
        if (!isPhase2)
        {
            isPhase2 = true; 

            // 공중 이동 시 비활성화
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // 1페이즈 행동 플래그 초기화
            isPerformingKick = isMovingToKick = isPerformingRanged = isPerformingMelee = isPerformingSwing = false;

            currentPhase2State = Phase2State.Uping;
            phase2Timer = 0f; // 타이머 초기화

            animator.SetTrigger("FlyUp");
            Debug.Log("보스: 2페이즈 시작! 공중으로 떠오릅니다.");
        }

        // 2페이즈의 현재 상태에 따라 다른 행동을 실행
        switch (currentPhase2State)
        {
            case Phase2State.Uping:
                HandleUping();
                break;
            case Phase2State.FlyContront:
                HandleFlyContront(); 
                break;
            case Phase2State.FlyAttack:
                HandlePhase2FlyAttack(); 
                break;
            case Phase2State.Landing:
                HandleLanding(); 
                break;
            case Phase2State.Resting:
                HandleResting(); 
                break;
        }
    }

    void HandleUping()
    {
        Vector3 targetPosition = new Vector3(transform.position.x, flyAltitude, transform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, 3f * Time.deltaTime);

        if (transform.position.y >= flyAltitude)
        {
            currentPhase2State = Phase2State.FlyContront;
            phase2Timer = 0f; 
        }
    }

    void HandleFlyContront()
    {
        phase2Timer += Time.deltaTime;

        if (phase2Timer >= upDuration)
        {
            currentPhase2State = Phase2State.FlyAttack;
            phase2Timer = 0f; 
            Debug.Log("원거리 공격 시작");
            Instantiate(groundEffect, new Vector3(transform.position.x, 0, transform.position.z), transform.rotation);
            return; 
        }

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; 
        directionToPlayer.Normalize();

        Vector3 orbitDirection = Vector3.Cross(directionToPlayer, Vector3.up);

        transform.position += orbitDirection * upConfrontSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, flyAltitude, transform.position.z);
    }

    void HandlePhase2FlyAttack()
    {
        phase2Timer += Time.deltaTime;

        if (phase2Timer <= chargeDuration)
        {
            if (!isCharging)
            {
                isCharging = true;
                animator.SetTrigger("Charge");
            }

            projectileSpawnTimer += Time.deltaTime;
            if (projectileSpawnTimer >= projectileSpawnInterval && currentProjectileCount < projectileCount)
            {
                SpawnPhase2Projectile();
                projectileSpawnTimer = 0f;
                currentProjectileCount++;
            }
        }
        else
        {
            if (!hasStartedLaunch)
            {
                hasStartedLaunch = true;
                animator.SetTrigger("FlyAttack"); 
                LaunchAllProjectiles();
            }

            if (phase2Timer >= chargeDuration + 1f)
            {
                //currentPhase2State = Phase2State.FlyContront;

                currentPhase2State = Phase2State.Landing;

                isLanding = true;
                landingTargetPosition = new Vector3(transform.position.x, 0f, transform.position.z);

                animator.SetTrigger("FlyDown");

                // 상태 초기화
                ResetPhase2Attack();
                phase2Timer = 0f;
            }
        }

    }

    void SpawnPhase2Projectile()
    {
        // 보스 주위 랜덤 위치 계산
        Vector2 randomCircle = Random.insideUnitCircle * projectileSpawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        spawnPosition.y = transform.position.y + 2f;

        GameObject projectile = Instantiate(phase2Projectile, spawnPosition, Quaternion.identity);

        if (projectile.GetComponent<Rigidbody>())
        {
            projectile.GetComponent<Rigidbody>().isKinematic = true; // 물리 영향 받지 않음
        }

        spawnedProjectiles.Add(projectile);
    }

    void LaunchAllProjectiles()
    {
        Vector3 playerPosition = player.position;

        foreach (GameObject projectile in spawnedProjectiles)
        {
            Vector2 randomOffset = Random.insideUnitCircle * projectileLaunchRadius;
            Vector3 targetPosition = playerPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;  

            Vector3 direction = (targetPosition - projectile.transform.position).normalized;

            // 약간의 위쪽 각도 추가 (포물선 궤도)
            direction.y = 0.3f; // 위쪽 각도 조절
            direction = direction.normalized;

            rb.linearVelocity = direction * 15f; // 속도 증가

            ParticleSystem ps = projectile.GetComponent<ParticleSystem>();
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
            }
            
        }

        spawnedProjectiles.Clear();
    }

    void ResetPhase2Attack()
    {
        // 모든 상태 초기화
        isCharging = false;
        hasStartedLaunch = false;
        currentProjectileCount = 0;
        projectileSpawnTimer = 0f;

        // 남은 투사체들 정리 (혹시 모를 경우를 대비)
        foreach (GameObject projectile in spawnedProjectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile);
            }
        }
        spawnedProjectiles.Clear();
    }

    void HandleLanding()
    {
        if (isLanding)
        {
            transform.position = Vector3.MoveTowards(transform.position, landingTargetPosition, flyDownSpeed * Time.deltaTime);

            if (transform.position.y <= 0.1f) // 약간의 여유값
            {
                transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

                isLanding = false;

                if (agent != null && !agent.enabled)
                {
                    agent.enabled = true;
                    agent.isStopped = true;
                }

                currentPhase2State = Phase2State.Resting;
                isResting = true;
                phase2Timer = 0f;

            }
        }
    }

    void HandleResting()
    {
        if (isResting)
        {
            phase2Timer += Time.deltaTime;

            if (phase2Timer >= restingDuration)
            {
                isResting = false;

                currentPhase2State = Phase2State.Uping;

                if (agent != null && agent.enabled)
                {
                    agent.isStopped = true;
                    agent.enabled = false;
                }

                phase2Timer = 0f;

                animator.SetTrigger("FlyUp");

            }
        }
    }

    bool IsPhase1()
    {
        MonsterState monsterState = GetComponent<MonsterState>();
        return monsterState.currentHP >= monsterState.maxHP * 0.5f;
    }

    void HandlePhase1Behavior(float distanceToPlayer)
    {
        if (currentState == Phase1State.Confronting)
        {
            HandleConfrontingBehavior();
        }

        if (distanceToPlayer >= veryFarDistance)
        {
            ApproachPlayer();
        }
        else if (distanceToPlayer >= farDistance && distanceToPlayer < veryFarDistance)
        {
            if (currentState == Phase1State.Walk)
            {
                EnterConfrontState();
            }
        }
    }

    void ApproachPlayer()
    {
        currentState = Phase1State.Walk;

        if (agent != null && agent.enabled)
        {
            agent.SetDestination(player.position);
            agent.isStopped = false;
        }

        UpdateAnimatorParameters();
        //Debug.Log("보스: 추격 상태 진입!");
    }

    void EnterConfrontState()
    {
        currentState = Phase1State.Confronting;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 대치 상태 진입시 행동 결정 
        DecideConfrontAction();
        confrontTimer = 0f;

        UpdateAnimatorParameters();
        //Debug.Log("보스: 대치 상태 진입!");
    }

    void DecideConfrontAction()
    {
        if (isPerformingKick || isMovingToKick) return; // 킥 공격중

        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                currentConfrontAction = ConfrontAction.Circling;
                //Debug.Log("보스: 공전하며 대치!");
                break;
            case 1:
                currentConfrontAction = ConfrontAction.KickAttack;
                StartKickAttack();

                //Debug.Log("보스: 킥 공격 선택!");
                break;
            case 2:
                currentConfrontAction = ConfrontAction.RangedAttack;
                //Debug.Log("보스: 원거리 공격 선택! ");
                StartRangedAttack();
                break;
        }
    }

    void StartKickAttack()
    {
        isMovingToKick = true;
        isPerformingKick = false;
        kickTimer = 0f;

        agent.isStopped = false;
        agent.speed = runSpeed;  
        agent.stoppingDistance = kickRange;
        agent.SetDestination(player.position);

        Instantiate(kickStartEffect, transform.position, transform.rotation);

    }

    void StartRangedAttack()
    {
        isPerformingRanged = true;
        rangedTimer = 0f;
        hasSpawnedProjectile = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        animator.SetTrigger("RangeAttack"); 
        
    }

    void HandleConfrontingBehavior()
    {
        bool isAttacking = isPerformingKick || isMovingToKick || isPerformingRanged ||
                           isPerformingMelee || isPerformingSwing;

        // 공격 중이 아닐 때만 타이머 업데이트
        if (!isAttacking)
        {
            confrontTimer += Time.deltaTime;

            if (confrontTimer >= confrontDecisionTime)
            {
                DecideConfrontAction();
                confrontTimer = 0f;
            }
        }

        // 현재 선택된 행동 실행
        switch (currentConfrontAction)
        {
            case ConfrontAction.Circling:
                HandleCirclingBehavior();
                break;
            case ConfrontAction.KickAttack:
                HandleKickAttack();
                break;
            case ConfrontAction.RangedAttack:
                HandleRangedAttack();
                break;
        }
    }

    void HandleRangedAttack()
    {
        if (isPerformingRanged)
        {
            rangedTimer += Time.deltaTime;

            if (!hasSpawnedProjectile && rangedTimer >= projectileSpawnTime)
            {
                SpawnProjectile();
                hasSpawnedProjectile = true;
            }

            // 원거리 공격 종료
            if (rangedTimer >= rangedAttackDuration)
            {
                isPerformingRanged = false;
                rangedTimer = 0f;
                confrontTimer = 0f;

                // 다시 공전 상태로
                currentConfrontAction = ConfrontAction.Circling;
                //Debug.Log("보스: 원거리 공격 종료, 공전 상태로 복귀");
            }
        }
    }

    void SpawnProjectile()
    {
        // 보스가 바라보는 방향의 앞쪽에 소환 위치 계산
        Vector3 spawnPosition = transform.position + transform.forward * 1f;

        spawnPosition.y = transform.position.y + 3f;

        // 불꽃 파티클 생성
        GameObject projectile = Instantiate(pase1Projectile, spawnPosition, transform.rotation);

        //Debug.Log("보스: 불꽃 공격 발동!");
    }

    void HandleKickAttack()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 킥 위치로 이동 중
        if (isMovingToKick && !isPerformingKick)
        {
            kickTimer += Time.deltaTime;

            agent.SetDestination(player.position);

            // 사거리에 도달했는지 확인
            if (distanceToPlayer <= kickRange)
            {
                // 킥 동작 시작
                isMovingToKick = false;
                isPerformingKick = true;
                kickTimer = 0f;

                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                animator.SetTrigger("Kick");
            }
            // 5초 동안 접근 못하면 포기
            else if (kickTimer >= kickApproachTimeout)
            {
                //Debug.Log("보스: 킥 공격 포기! 플레이어가 너무 멀어짐");

                // 킥 공격 취소
                isMovingToKick = false;
                isPerformingKick = false;
                kickTimer = 0f;
                confrontTimer = 0f;

                // agent 정지
                if (agent != null)
                {
                    agent.isStopped = true;
                }

                // 다시 공전 상태로
                currentConfrontAction = ConfrontAction.Circling;
            }
        }
        else if (isPerformingKick)
        {
            kickTimer += Time.deltaTime;

            if (kickTimer >= kickDuration)
            {
                isPerformingKick = false;
                kickTimer = 0f;
                confrontTimer = 0f;

                currentConfrontAction = ConfrontAction.Circling;
            }
        }
    }

    void HandleCirclingBehavior()
    {
        if (isPerformingMelee)
        {
            HandleLeftHookAttack();
            return;
        }
        if (isPerformingSwing)
        {
            HandleSwingAttack();
            return;
        }
        if (isPerformingRanged)
        {
            HandleRangedAttack();
            return;
        }

        float currentDistance = Vector3.Distance(transform.position, player.position);
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 Direction = Vector3.Cross(Vector3.up, directionToPlayer);

        if (currentDistance < backStepDistance)
        {
            if (!hasDecidedCloseAction)
            {
                hasDecidedCloseAction = true;

                if (Random.Range(0, 2) == 0)
                {
                    isBackStep = true;
                    //Debug.Log("보스 백스텝");
                }
                else
                {
                    isBackStep = false;
                    StartLeftHookAttack();
                    //Debug.Log("보스 레프트훅");
                }

            }

            if (isBackStep)
            {
                Vector3 backDirection = -directionToPlayer;
                transform.position += backDirection * backStepSpeed * Time.deltaTime;
            }
        }
        else
        {
            hasDecidedCloseAction = false;
            isBackStep = false;

            transform.position += Direction * confrontSpeed * Time.deltaTime;
        }
    }
    void StartLeftHookAttack()
    {
        isPerformingMelee = true;
        meleeTimer = 0f;

        leftHookDirection = (player.position - transform.position).normalized;
        leftHookDirection.y = 0;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        animator.SetTrigger("LeftHook");
    }

    void HandleLeftHookAttack()
    {
        meleeTimer += Time.deltaTime;

        transform.position += leftHookDirection * leftHookMoveSpeed * Time.deltaTime;

        if (meleeTimer >= leftHookDuration)
        {
            isPerformingMelee = false;
            meleeTimer = 0f;
            StartSwingAttack();
        }
    }

    void StartSwingAttack()
    {
        isPerformingSwing = true;
        swingTimer = 0f;
        swingStartPos = transform.position;
        swingDirection = (player.position - transform.position).normalized;

        animator.SetTrigger("Swing");
    }

    void HandleSwingAttack()
    {
        swingTimer += Time.deltaTime;
        float t = Mathf.Min(swingTimer / swingDuration, 1f);
        transform.position = swingStartPos + swingDirection * (swingForwardDistance * t);

        if (swingTimer >= swingDuration)
        {
            // 스윙 끝나면 상태 초기화
            isPerformingSwing = false;
            swingTimer = 0f;
            hasDecidedCloseAction = false;

        }
    }

    void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        float currentSpeed = 0f;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isMovingRight", false);
        animator.SetBool("isWalkingBack", false);

        if (!isPhase2)
        {
            if (currentState == Phase1State.Walk)
            {
                currentSpeed = agent.velocity.magnitude;
                animator.SetFloat("Speed", currentSpeed);
                animator.SetBool("isWalking", currentSpeed > 0.1f);
            }
            else if (currentState == Phase1State.Confronting)
            {
                // 킥 공격 중일 때
                if (currentConfrontAction == ConfrontAction.KickAttack)
                {
                    if (isMovingToKick)
                    {
                        // 킥 위치로 이동 중
                        currentSpeed = agent.velocity.magnitude;
                        animator.SetFloat("Speed", currentSpeed);
                        if (currentSpeed > 0.1f)
                        {
                            animator.SetBool("isRunning", true);
                            animator.SetBool("isWalking", false);
                        }
                    }
                    else if (isPerformingKick)
                    {
                        // 킥 동작 중
                        animator.SetFloat("Speed", 0);
                    }
                }
                else if (currentConfrontAction == ConfrontAction.RangedAttack)
                {
                    if (isPerformingRanged)
                    {
                        // 원거리 공격 동작 중
                        animator.SetFloat("Speed", 0);
                    }
                }
                // 공전 중일 때
                else
                {
                    animator.SetFloat("Speed", 0);
                    if (isBackStep)
                        animator.SetBool("isWalkingBack", true);
                    else
                        animator.SetBool("isMovingRight", true);
                }
            }
        }
        else    // 2페이즈
        {
            animator.SetBool("isFlyingLeft", false);
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetBool("isMovingRight", false);
            animator.SetBool("isWalkingBack", false);
            switch (currentPhase2State)
            {
                case Phase2State.FlyContront:
                    animator.SetBool("isFlyingLeft", true);
                    break;
                case Phase2State.Resting:
                    animator.SetFloat("Speed", 0);
                    break;
            }
            return;
        }

    }

    void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
