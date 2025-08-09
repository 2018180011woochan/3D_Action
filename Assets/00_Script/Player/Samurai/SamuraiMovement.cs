using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SamuraiMovement : MonoBehaviour
{
    // [ 요약 ]
    // Update() 함수가 매 프레임마다 아래 함수들을 순서대로 호출하며 지휘하는 구조입니다.
    // 1. ProcessInputs()      : 키보드, 마우스 입력을 받아 상태 변수에 저장합니다.
    // 2. ProcessState()       : 입력값에 따라 isDashing 같은 상태를 변경합니다.
    // 3. ProcessMovement()    : 상태에 따라 캐릭터를 움직이고 회전시킵니다.
    // 4. ProcessAnimations()  : 계산된 값들을 애니메이터에 전달합니다.

    [Header("상태 변수")]
    public bool Stance = false;
    public bool isBusy = false;
    private bool isRunning;
    private bool jumpInput;
    private bool dashInput; // ★ 대시 입력 변수

    [Header("이동 설정")]
    public float moveSpeed = 2.0f;
    public float combatMoveSpeed = 2.0f;
    public float runSpeed = 4.0f;
    public float rotationSpeed = 15.0f;
    public float jumpHeight = 1.5f;

    [Header("대시 설정")]
    public float dashSpeed = 10.0f;   // ★ 대시 속도
    public float dashDuration = 0.3f; // ★ 대시 지속 시간
    private bool isDashing = false;   // ★ 현재 대시 중인지 확인
    private float dashTimer;          // ★ 대시 남은 시간을 체크할 타이머
    private Vector3 dashDirection;    // ★ 대시 방향

    [Header("중력")]
    private Vector3 verticalVelocity;
    private bool isGrounded;
    private float gravityValue = -9.81f;

    [Header("컴포넌트")]
    private Animator animator;
    private CharacterController controller;
    public Transform cameraTransform;

    // 입력 값을 저장할 변수
    private Vector2 moveInput;

    // 애니메이터 파라미터 해시
    private readonly int hashIsStance = Animator.StringToHash("IsStance");
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashIsRun = Animator.StringToHash("Run");
    private readonly int hashMoveX = Animator.StringToHash("MoveX");
    private readonly int hashMoveY = Animator.StringToHash("MoveY");
    private readonly int hashDraw = Animator.StringToHash("Draw");
    private readonly int hashJump = Animator.StringToHash("Jump");
    private readonly int hashDash = Animator.StringToHash("Dash");
    private readonly int hashIsDashing = Animator.StringToHash("isDashing");
    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 매 프레임, 정해진 순서대로 함수를 실행
        ProcessInputs();
        ProcessState();
        ProcessMovement();
        ProcessAnimations();
    }

    private void ProcessInputs()
    {
        if (isBusy)
        {
            moveInput = Vector2.zero;
            isRunning = false;
            jumpInput = false;
            dashInput = false;
            return;
        }

        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        isRunning = Input.GetKey(KeyCode.LeftShift);
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        dashInput = Input.GetKeyDown(KeyCode.LeftControl); // ★ 대시 입력 받기

        if (!Stance && Input.GetMouseButtonDown(0))
        {
            isBusy = true;
            animator.SetTrigger(hashDraw);
        }
    }

    // ★ 상태를 관리하는 함수 추가
    private void ProcessState()
    {
        // 대시 상태 관리
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                isBusy = false; // 대시가 끝나면 다른 행동 가능
            }
        }
        // 새로운 대시 시작
        else if (dashInput && isGrounded && !isBusy)
        {
            isDashing = true;
            isBusy = true; // 대시 중에는 다른 행동 못함
            dashTimer = dashDuration;
            dashDirection = GetMoveDirection();
            if (dashDirection == Vector3.zero)
            {
                // 가만히 서있을 땐 캐릭터가 보는 방향으로 대시
                dashDirection = transform.forward;
            }
        }
    }

    private void ProcessMovement()
    {
        Vector3 finalVelocity;

        // 대시 중일 때의 움직임
        if (isDashing)
        {
            // 대시 중에는 중력을 무시하고, 정해진 방향과 속도로만 이동
            finalVelocity = dashDirection * dashSpeed;
        }
        // 평상시 움직임
        else
        {
            // 중력 처리
            isGrounded = controller.isGrounded;
            if (isGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2.0f;
            }
            verticalVelocity.y += gravityValue * Time.deltaTime;

            // 점프 처리
            if (jumpInput && isGrounded)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            }

            // 수평 이동 처리
            Vector3 moveDirection = GetMoveDirection();
            float currentSpeed = GetCurrentSpeed();
            finalVelocity = moveDirection * currentSpeed + verticalVelocity;
        }

        // 모든 계산이 끝난 후, Move를 딱 한 번만 호출!
        controller.Move(finalVelocity * Time.deltaTime);

        // 대시 중이 아닐 때만 회전 처리
        if (!isDashing)
        {
            ProcessRotation();
        }
    }

    private void ProcessAnimations()
    {
        animator.SetBool(hashIsStance, Stance);
        animator.SetBool(hashIsDashing, isDashing); // ★ 이 줄을 추가해주세요.

        float speed = new Vector2(moveInput.x, moveInput.y).magnitude;
        animator.SetFloat(hashSpeed, speed);

        bool canRun = isRunning && moveInput.y > 0;
        animator.SetBool(hashIsRun, canRun);

        if (Stance)
        {
            Vector3 localMoveDir = transform.InverseTransformDirection(GetMoveDirection());
            animator.SetFloat(hashMoveX, localMoveDir.x);
            animator.SetFloat(hashMoveY, localMoveDir.z);
        }

        if (jumpInput && isGrounded)
        {
            animator.SetTrigger(hashJump);
        }
    }

    // --- 유틸리티 함수들 ---

    private float GetCurrentSpeed()
    {
        bool canRun = isRunning && moveInput.y > 0;
        float speed = Stance ? combatMoveSpeed : moveSpeed;
        return canRun ? runSpeed : speed;
    }

    private void ProcessRotation()
    {
        Vector3 directionToRotate = GetMoveDirection();
        if (Stance)
        {
            directionToRotate = GetCameraForward();
        }

        if (directionToRotate == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(directionToRotate);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private Vector3 GetMoveDirection()
    {
        Vector3 cameraForward = GetCameraForward();
        Vector3 cameraRight = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;
        return (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;
    }

    private Vector3 GetCameraForward()
    {
        return new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
    }

    public void OnDrawEnd()
    {
        Stance = true;
        isBusy = false;
    }
}