using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class KerriganPhase2 : MonoBehaviour
{
    // 상태 정의
    public enum State
    {
        FlyUp,
        FlyOrbit,
        FlyAttack,
        Landing,
        Rest
    }
    private State currentState = State.FlyUp;

    [Header("비행 설정")]
    public float flyAltitude = 8f;
    public float flyUpSpeed = 3f;
    public float orbitSpeed = 4f;
    public float orbitDuration = 5f;

    [Header("공격 설정")]
    public int projectileCount = 10;
    public float projectileSpawnRadius = 8f;
    public float projectileSpawnInterval = 0.5f;
    public GameObject projectilePrefab;
    public GameObject groundEffectPrefab;

    [Header("착지 설정")]
    public float landingSpeed = 5f;
    public float restDuration = 3f;

    private float stateTimer = 0f;
    private float projectileSpawnTimer = 0f;
    private int spawnedProjectileCount = 0;
    private List<GameObject> spawnedProjectiles = new List<GameObject>();

    // 메인 AI 참조
    private BossAI bossAI;

    void Awake()
    {
        bossAI = GetComponent<BossAI>();
    }

    void Update()
    {
        if (bossAI.IsHit())
        {
            Debug.Log("Phase2: GetHit 중이므로 Update 중지");
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
            case State.FlyAttack:
                UpdateFlyAttack();
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
        Debug.Log("상승시작");
        // 상승
        Vector3 targetPos = new Vector3(transform.position.x, flyAltitude, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, flyUpSpeed * Time.deltaTime);

        // 고도 도달
        if (transform.position.y >= flyAltitude - 0.1f)
        {
            ChangeState(State.FlyOrbit);
        }
    }

    void UpdateFlyOrbit()
    {
        // 공중에서 공전
        Vector3 direction = (bossAI.Player.position - transform.position).normalized;
        direction.y = 0;
        Vector3 orbitDir = Vector3.Cross(direction, Vector3.up);

        transform.position += orbitDir * orbitSpeed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, flyAltitude, transform.position.z);

        bossAI.Animator.SetBool("isFlyingLeft", true);

        // 일정 시간 후 공격
        if (stateTimer >= orbitDuration)
        {
            ChangeState(State.FlyAttack);
        }
    }

    void UpdateFlyAttack()
    {
        // 투사체 생성 단계
        if (spawnedProjectileCount < projectileCount)
        {
            projectileSpawnTimer += Time.deltaTime;

            if (projectileSpawnTimer >= projectileSpawnInterval)
            {
                SpawnProjectile();
                projectileSpawnTimer = 0f;
            }
        }
        // 발사 단계
        else if (stateTimer >= 5f) // 5초 후 발사
        {
            LaunchAllProjectiles();
            ChangeState(State.Landing);
        }
    }

    void UpdateLanding()
    {
        // 착지
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
        // 휴식
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
        spawnedProjectiles.Add(proj);
        spawnedProjectileCount++;
    }

    void LaunchAllProjectiles()
    {
        Vector3 playerPos = bossAI.Player.position;

        foreach (GameObject proj in spawnedProjectiles)
        {
            if (proj == null) continue;

            // 플레이어 주변으로 발사
            Vector2 randomOffset = Random.insideUnitCircle * 3f;
            Vector3 targetPos = playerPos + new Vector3(randomOffset.x, 0, randomOffset.y);

            // 투사체 발사 로직
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
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
        Debug.Log($"Phase2 상태 변경: {newState}");
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

            case State.FlyAttack:
                spawnedProjectileCount = 0;
                projectileSpawnTimer = 0f;
                bossAI.Animator.SetTrigger("Charge");
                if (groundEffectPrefab != null)
                {
                    Vector3 groundPos = new Vector3(transform.position.x, 0, transform.position.z);
                    Instantiate(groundEffectPrefab, groundPos, Quaternion.identity);
                }
                break;

            case State.Landing:
                bossAI.Animator.SetTrigger("FlyDown");
                break;

            case State.Rest:
                // NavMeshAgent 재활성화
                if (bossAI.Agent != null && !bossAI.Agent.enabled)
                {
                    bossAI.Agent.enabled = true;
                    bossAI.Agent.isStopped = true;
                }
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
        }
    }

    public void OnPhaseEnter()
    {
        // Phase2 진입시 NavMeshAgent 비활성화
        if (bossAI.Agent != null && bossAI.Agent.enabled)
        {
            bossAI.Agent.isStopped = true;
            bossAI.Agent.enabled = false;
        }

        ChangeState(State.FlyUp);
    }

    void OnEnable()
    {
        Debug.Log("Phase2 컴포넌트 활성화!");
        StartCoroutine(StartPhase2WhenReady());
    }

    IEnumerator StartPhase2WhenReady()
    {
        // GetHit 중이면 대기
        while (bossAI != null && bossAI.IsHit())
        {
            Debug.Log("Phase2: 활성화되었지만 GetHit 대기 중...");
            yield return null;
        }

        Debug.Log("Phase2: 시작!");
        OnPhaseEnter();
    }
}