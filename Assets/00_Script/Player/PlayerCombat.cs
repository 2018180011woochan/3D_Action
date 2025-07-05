using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("콤보 설정")]
    public int maxCombo = 3;               // 최대 콤보 단계
    public float comboResetTime = 1f;      // 마지막 공격 후 이 시간 안에 클릭하면 콤보 이어감

    private int comboStep = 0;             // 현재 콤보 단계 (0=Idle, 1~3=공격1~3)
    private float comboTimer = 0f;         // 단계 유지 용 타이머

    private Animator animator;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1) 타이머 감소 & 만료 시 콤보 초기화
        if (comboTimer > 0f)
            comboTimer -= Time.deltaTime;
        else
            comboStep = 0;  // Idle 상태로

        // 2) 좌클릭 입력 처리
        if (Input.GetMouseButtonDown(0))
            HandleComboClick();
    }

    void HandleComboClick()
    {
        switch (comboStep)
        {
            case 0:
                comboStep = 1;
                animator.SetTrigger("Attack1");
                break;
            case 1:
                comboStep = 2;
                animator.SetTrigger("Attack2");
                break;
            case 2:
                comboStep = 3;
                animator.SetTrigger("Attack3");
                break;
            default:
                // comboStep == 3 일 때는 더 이상 단계 없음
                return;
        }

        // 클릭할 때마다 타이머 리셋
        comboTimer = comboResetTime;
    }
}
