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

    void Awake()
    {
        // 컴포넌트 초기화
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        MonsterState = GetComponent<MonsterState>();

        // 페이즈 컴포넌트 가져오기
        phase1 = GetComponent<KerriganPhase1>();
        phase2 = GetComponent<KerriganPhase2>();
        phase1.enabled = true;
        phase2.enabled = false;             // 처음에는 비활성화 
    }

    void Update()
    {
        if (IsDead()) return;
        if (IsHit()) return;

        CheckPhaseTransition();

        LookAtPlayer();
    }

    void CheckPhaseTransition()
    {
        // 체력 50 미만이면 2페이즈로 전환
        if (!isPhase2Active && MonsterState.currentHP < 50f)
        {
            TransitionToPhase2();
        }
    }

    void TransitionToPhase2()
    {
        isPhase2Active = true;

        // Phase1 종료
        phase1.OnPhaseExit();
        phase1.enabled = false;
        
        // Phase2 시작
        phase2.enabled = true;
        phase2.OnPhaseEnter();
        
        Debug.Log("2페이즈 전환!");
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