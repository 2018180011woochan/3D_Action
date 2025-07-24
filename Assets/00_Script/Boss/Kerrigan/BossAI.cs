using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    [Header("컴포넌트")]
    public NavMeshAgent Agent { get; private set; }
    public Animator Animator { get; private set; }
    public Transform Player;
    public MonsterState MonsterState { get; private set; }

    [Header("거리 설정")]
    public float veryFarDistance = 20f;
    public float farDistance = 10f;
    public float closeDistance = 3f;

    [Header("이동 속도")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float confrontSpeed = 1.5f;

    // 페이즈 관리
    private KerriganPhase1 phase1;
    private KerriganPhase2 phase2;
    private bool isPhase2Active = false;

    private bool isTransitioning = false;
    private bool isInitialized = false;  // 초기화 완료 체크

    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        // Start에서 한 번 더 체크
        if (!isInitialized)
        {
            InitializeComponents();
        }
    }

    void InitializeComponents()
    {
        // 컴포넌트 초기화
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        MonsterState = GetComponent<MonsterState>();

        // null 체크
        if (Agent == null || Animator == null || MonsterState == null)
        {
            Debug.LogError("필수 컴포넌트가 없습니다!");
            return;
        }

        // 페이즈 컴포넌트 가져오기
        phase1 = GetComponent<KerriganPhase1>();
        phase2 = GetComponent<KerriganPhase2>();

        if (phase1 != null) phase1.enabled = true;
        if (phase2 != null) phase2.enabled = false;

        // 플레이어 찾기
        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Player = playerObj.transform;
            }
        }

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;
        if (IsDead()) return;
        if (IsHit()) return;

        CheckPhaseTransition();

        LookAtPlayer();
    }

    void CheckPhaseTransition()
    {
        if (isTransitioning) return;
        // 체력 50 미만이면 2페이즈로 전환
        if (!isPhase2Active && MonsterState.currentHP < 50f)
        {
            StartCoroutine(TransitionToPhase2AfterHit());
        }
    }

    IEnumerator TransitionToPhase2AfterHit()
    {
        isTransitioning = true;

        while (Animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
        {
            yield return null; 
        }

        TransitionToPhase2();
        isTransitioning = false;
    }

    void TransitionToPhase2()
    {
        isPhase2Active = true;

        // Phase1 종료
        phase1.OnPhaseExit();
        phase1.enabled = false;
        
        // Phase2 시작
        phase2.enabled = true;
    }

    public float GetDistanceToPlayer()
    {
        if (Player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, Player.position);
    }

    public void LookAtPlayer()
    {
        if (Player == null) return;

        Vector3 direction = Player.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public bool IsDead()
    {
        return MonsterState.currentHP <= 0;
    }

    public bool IsHit()
    {
        return Animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit");
    }
}