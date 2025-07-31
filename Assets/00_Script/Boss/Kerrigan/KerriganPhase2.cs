using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class KerriganPhase2 : MonoBehaviour
{
    public enum State
    {
        FlyUp,
        FlyOrbit,
        ProjectileRain,    
        FireBreath,        
        GroundSlam,       
        Landing,
        Rest
    }
    private State currentState = State.FlyUp;
    private List<State> attackPatterns = new List<State>();
    private int currentAttackIndex = 0;

    [Header("공전 관련")]
    public float flyAltitude = 8f;
    public float flyUpSpeed = 3f;
    public float orbitSpeed = 4f;
    public float orbitDuration = 2f;

    [Header("공 던지기 관련")]
    public int projectileCount = 20;
    public float projectileSpawnRadius = 8f;
    public float projectileSpawnInterval = 0.25f;
    public GameObject projectilePrefab;
    public GameObject groundEffectPrefab;

    [Header("불 뿜기 관련")]
    public GameObject FireBreathProjectile;
    private GameObject activeFireBreath;
    private List<GameObject> fireBreathList = new List<GameObject>();
    private bool fireBreathCreated = false;
    private float fireBreathExtendTimer = 0f;
    private int currentFireIndex = 0;

    public GameObject FireAreaPrefab;  // 추가
    public float fireAreaRadius = 10f;  // 추가
    public int fireAreaCount = 24;     // 원 둘레에 생성할 개수 (추가)
    private int currentFireAreaIndex = 0;  // 추가
    private float fireAreaSpawnTimer = 0f;  // 추가

    private float fireBreathShootTimer = 0f;

    [Header("랜딩")]
    public float landingSpeed = 5f;
    public float restDuration = 3f;

    private float stateTimer = 0f;
    private float projectileSpawnTimer = 0f;
    private int spawnedProjectileCount = 0;
    private List<GameObject> spawnedProjectiles = new List<GameObject>();

    private BossAI bossAI;

    // 무적 state
    private List<State> invincibleStates = new List<State> { State.FlyUp };

    void Awake()
    {
        bossAI = GetComponent<BossAI>();
        InitializeAttackPatterns();
    }

    void InitializeAttackPatterns()
    {
        attackPatterns.Add(State.ProjectileRain);
        attackPatterns.Add(State.FireBreath);
        attackPatterns.Add(State.GroundSlam);
    }

    void Update()
    {
        if (bossAI.IsHit())
        {
            return;
        }

        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case State.FlyUp:
                UpdateFlyUp();
                break;
            case State.FlyOrbit:
                UpdateFlyOrbit();
                break;
            case State.ProjectileRain:
                UpdateProjectileRain();
                break;
            case State.FireBreath:
                UpdateFireBreath();
                break;
            case State.GroundSlam:
                UpdateGroundSlam();
                break;
            case State.Landing:
                UpdateLanding();
                break;
            case State.Rest:
                UpdateRest();
                break;
        }
    }

    void UpdateFlyUp()
    {
        Vector3 targetPos = new Vector3(transform.position.x, flyAltitude, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, flyUpSpeed * Time.deltaTime);

        if (transform.position.y >= flyAltitude - 0.1f)
        {
            ChangeState(State.FlyOrbit);
        }
    }

    void UpdateFlyOrbit()
    {
        Vector3 direction = (bossAI.Player.position - transform.position).normalized;
        direction.y = 0;
        Vector3 orbitDir = Vector3.Cross(direction, Vector3.up);

        transform.position += orbitDir * orbitSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, flyAltitude, transform.position.z);

        bossAI.Animator.SetBool("isFlyingLeft", true);

        if (stateTimer >= orbitDuration)
        {
            ChangeState(SelectNextAttack());
        }
    }

    public bool IsInvincible()
    {
        return invincibleStates.Contains(currentState);
    }

    State SelectNextAttack()
    {
        State selectedAttack;

        selectedAttack = attackPatterns[currentAttackIndex];
        currentAttackIndex = (currentAttackIndex + 1) % attackPatterns.Count;
        
        Debug.Log($"���õ� ���� ����: {selectedAttack}");
        //return selectedAttack;
        return State.FireBreath;    // test
    }

    void UpdateProjectileRain()
    {
        if (spawnedProjectileCount < projectileCount)
        {
            projectileSpawnTimer += Time.deltaTime;

            if (projectileSpawnTimer >= projectileSpawnInterval)
            {
                SpawnProjectile();
                projectileSpawnTimer = 0f;
            }
        }
        else if (stateTimer >= 5f) 
        {
            LaunchAllProjectiles();
            ChangeState(State.Landing);
        }
    }

    void UpdateFireBreath()
    {
        Vector3 playerPosition = bossAI.Player.position;
        Vector3 bossPosition = transform.position;
        Vector3 directionToPlayer = playerPosition - bossPosition;

        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }

        fireBreathShootTimer += Time.deltaTime;

        if (fireBreathShootTimer >= 0.1f)
        {
            Vector3 spawnPosition = bossPosition + transform.forward * 2f; 
            GameObject newFire = PoolManager.Instance.GetFireBreathProjectile();

            if (newFire != null)
            {
                newFire.transform.position = spawnPosition;
                newFire.transform.rotation = transform.rotation;

                Rigidbody rb = newFire.GetComponent<Rigidbody>();
                rb.linearVelocity = directionToPlayer.normalized * 20f;

                // 3초 후 자동 반환
                StartCoroutine(ReturnFireProjectileAfterDelay(newFire, 3f));
            }

            fireBreathShootTimer = 0f; 
        }

        // fireArea
        fireAreaSpawnTimer += Time.deltaTime;
        if (fireAreaSpawnTimer >= 0.1f && currentFireAreaIndex < fireAreaCount)
        {
            // 원형으로 배치할 각도 계산
            float angle = (360f / fireAreaCount) * currentFireAreaIndex;
            float radian = angle * Mathf.Deg2Rad;

            // 보스 주위 원형 위치 계산
            Vector3 fireAreaPosition = new Vector3(
                bossPosition.x + Mathf.Cos(radian) * fireAreaRadius,
                0f,  // 바닥에 생성
                bossPosition.z + Mathf.Sin(radian) * fireAreaRadius
            );

            GameObject fireArea = PoolManager.Instance.GetFireArea();
            if (fireArea != null)
            {
                fireArea.transform.position = fireAreaPosition;
                fireArea.transform.rotation = Quaternion.identity;

                // 5초 후 자동 반환
                StartCoroutine(ReturnFireAreaAfterDelay(fireArea, 5f));
            }

            currentFireAreaIndex++;
            fireAreaSpawnTimer = 0f;
        }

        if (stateTimer >= 5f)
        {
            ChangeState(State.Landing);
        }
    }
    IEnumerator ReturnFireProjectileAfterDelay(GameObject projectile, float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolManager.Instance.ReturnFireBreathProjectile(projectile);
    }

    IEnumerator ReturnFireAreaAfterDelay(GameObject fireArea, float delay)
    {
        yield return new WaitForSeconds(delay);
        PoolManager.Instance.ReturnFireArea(fireArea);
    }

    void UpdateGroundSlam()
    {
       
    }

    void UpdateLanding()
    {
        Vector3 landingPos = new Vector3(transform.position.x, 0f, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, landingPos, landingSpeed * Time.deltaTime);

        if (transform.position.y <= 0.1f)
        {
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
            ChangeState(State.Rest);
        }
    }

    void UpdateRest()
    {
        if (stateTimer >= restDuration)
        {
            ChangeState(State.FlyUp);
        }
    }

    void SpawnProjectile()
    {
        if (projectilePrefab == null) return;

        Vector2 randomCircle = Random.insideUnitCircle * projectileSpawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;  
            rb.isKinematic = true;  
        }

        spawnedProjectiles.Add(proj);
        spawnedProjectileCount++;
    }

    void LaunchAllProjectiles()
    {
        Vector3 playerPos = bossAI.Player.position;

        foreach (GameObject proj in spawnedProjectiles)
        {
            if (proj == null) continue;

            Vector2 randomOffset = Random.insideUnitCircle * 3f;
            Vector3 targetPos = playerPos + new Vector3(randomOffset.x, 0, randomOffset.y);

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;  
                rb.useGravity = true;    

                Vector3 direction = (targetPos - proj.transform.position).normalized;
                direction.y = 0.3f;
                rb.linearVelocity = direction.normalized * 15f;
            }
        }

        spawnedProjectiles.Clear();
    }

    void ChangeState(State newState)
    {
        ExitState(currentState);
        currentState = newState;
        stateTimer = 0f;
        EnterState(newState);
    }

    void EnterState(State state)
    {
        switch (state)
        {
            case State.FlyUp:
                bossAI.Animator.SetTrigger("FlyUp");
                break;

            case State.FlyOrbit:
                break;

            case State.ProjectileRain:
                spawnedProjectileCount = 0;
                projectileSpawnTimer = 0f;
                bossAI.Animator.SetTrigger("Charge");
                if (groundEffectPrefab != null)
                {
                    Vector3 groundPos = new Vector3(transform.position.x, 0, transform.position.z);
                    Instantiate(groundEffectPrefab, groundPos, Quaternion.identity);
                }
                break;
            case State.FireBreath:
                activeFireBreath = Instantiate(FireBreathProjectile, transform);
                break;

            case State.Landing:
                bossAI.Animator.SetTrigger("FlyDown");
                break;

            case State.Rest:
                bossAI.Agent.enabled = true;
                bossAI.Agent.isStopped = true;
                
                break;
        }
    }

    void ExitState(State state)
    {
        switch (state)
        {
            case State.FlyOrbit:
                bossAI.Animator.SetBool("isFlyingLeft", false);
                break;
            case State.FireBreath:
                fireBreathShootTimer = 0f;
                fireAreaSpawnTimer = 0f;    // 추가
                currentFireAreaIndex = 0;    // 추가
                break;
        }
    }

    void OnEnable()
    {
        StartCoroutine(StartPhase2WhenReady());
    }

    IEnumerator StartPhase2WhenReady()
    {
        while (bossAI.IsHit())
        {
            yield return null;
        }

        bossAI.Agent.isStopped = true;
        bossAI.Agent.enabled = false;
        ChangeState(State.FlyUp);
    }
}