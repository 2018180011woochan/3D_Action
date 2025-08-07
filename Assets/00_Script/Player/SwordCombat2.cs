using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using Drakkar.GameUtils;
using UnityEngine.Timeline;
using Unity.Cinemachine;

public class SwordCombat2 : MonoBehaviour
{
    private Animator animator;

    public enum PlayerState
    {
        Idle,
        Attacking
    }
    public PlayerState currentState = PlayerState.Idle;
    private int comboStep = 0; // 현재 콤보 단계 (0: 콤보 시작 전, 1: 1타, 2: 2타...)
    private bool canContinueCombo = false; // 다음 콤보로 이어질 수 있는 '창구'가 열렸는지
    private bool isComboBuffered = false;  // '창구'가 열렸을 때 플레이어가 입력을 했는지 (입력 예약)

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleAttackInput();
        }
    }
    private void HandleAttackInput()
    {
        // 1. 만약 '콤보 창구'가 열려있다면... (공격 중 다음 입력)
        if (canContinueCombo)
        {
            isComboBuffered = true; // 다음 공격을 하겠다고 '예약'만 해둡니다.
            return;
        }

        // 2. '대기' 상태일 때만 첫 공격이 나갑니다.
        if (currentState == PlayerState.Idle)
        {
            comboStep = 1;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        currentState = PlayerState.Attacking; // 공격 시작! 상태 변경
        isComboBuffered = false; // 공격이 시작되었으니 예약은 초기화

        // 콤보 단계에 맞는 애니메이션 트리거를 발동시킵니다.
        // "Attack" + 1 -> "Attack1", "Attack" + 2 -> "Attack2" ...
        //animator.SetTrigger("Attack" + comboStep);
        animator.Play("slash" + comboStep, -1, 0f);
    }
    private void ResetCombo()
    {
        currentState = PlayerState.Idle;
        comboStep = 0;
        canContinueCombo = false;
        isComboBuffered = false;
    }
    public void OpenComboWindow()
    {
        Debug.Log(comboStep);
        canContinueCombo = true; // "이제 다음 공격 입력 받습니다!"
    }

    // [새로 추가] '다음 콤보 입력이 가능한 구간' 종료 시점에 호출
    public void CloseComboWindow()
    {
        canContinueCombo = false; // "입력 시간 끝!"

        // 창구가 닫힐 때, 버퍼에 다음 공격이 '예약'되어 있었다면...
        if (isComboBuffered)
        {
            comboStep++;
            PerformAttack(); // 다음 콤보 공격 실행!
        }
        // 예약된 공격이 없다면, 콤보는 여기서 끝.
        else
        {
            ResetCombo();
        }
    }
}