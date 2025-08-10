using UnityEngine;

public class SamuraiMovement : MonoBehaviour
{
    [Header("상태 변수")]
    public bool Stance = false;
    public bool isBusy = false;    // 현재 플레이어가 다른 작업 중인지

    [Header("상태 변수")]
    public float moveSpeed = 2.0f;
    public float combatMoveSpeed = 2.0f;
    public float rotationSpeed = 10.0f;
    public float gravity = -9.81f;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private float gravityValue = -9.81f;
    public float runSpeed = 3.0f;
    private bool isRunning = false;
    public float jumpHeight = 1.5f;

    [Header("대시")]
    public KeyCode dashKey = KeyCode.LeftControl;
    public float dashSpeed = 8f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.3f;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection = Vector3.zero;

    [Header("컴포넌트")]
    private Animator animator;
    private CharacterController controller;
    public Transform cameraTransform;

    bool isFirstDraw = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        ApplyGravity();

        HandleDash();

        animator.SetBool("IsStance", Stance);
        if (isBusy)
        {
            animator.SetBool("Run", false);
            animator.SetFloat("Speed", 0f);
            return;
        }

        isRunning = Input.GetKey(KeyCode.LeftShift);
        HandleJump();
        HandleDraw();
        if (Stance == false)
        {
            HandleMove();
        }
        else
        {
            HandleCombatMove();
        }
    }
    private void ApplyGravity()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
    }
    private void HandleMove()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;

        Vector3 moveDirection = (cameraForward.normalized * vertical + cameraRight.normalized * horizontal).normalized;

        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        animator.SetFloat("Speed", moveDirection.magnitude);
        animator.SetBool("Run", isRunning);
        //controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        Vector3 finalVelocity = moveDirection * currentSpeed;
        finalVelocity.y = playerVelocity.y;
        controller.Move(finalVelocity * Time.deltaTime);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleCombatMove()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        Vector3 moveDirection = (cameraForward.normalized * vertical + cameraRight.normalized * horizontal).normalized;

        bool canRun = isRunning && vertical > 0;
        float currentSpeed = canRun ? runSpeed : moveSpeed;

        animator.SetFloat("Speed", moveDirection.magnitude);
        animator.SetBool("Run", canRun);

        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

        animator.SetFloat("MoveX", localMoveDirection.x);
        animator.SetFloat("MoveY", localMoveDirection.z);

        //controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        Vector3 finalVelocity = moveDirection * currentSpeed;
        finalVelocity.y = playerVelocity.y;

        controller.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleDraw()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (Stance == false)
            {
                if (isFirstDraw)
                {
                    animator.SetTrigger("Draw");
                    isBusy = true;
                    isFirstDraw = false;
                }
                else
                    Stance = true;
            }
            else
            {
                Stance = false;
                animator.SetBool("IsStance", false);
            }
        }
    }
    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            animator.SetTrigger("Jump");

            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
        }
    }

    private void HandleDash()
    {
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;

        if (isDashing)
        {
            Vector3 final = dashDirection * dashSpeed;
            final.y = playerVelocity.y; 
            controller.Move(final * Time.deltaTime);

            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
            }
            return;
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0f && !isBusy && v > 0.1f)
        {
            Vector3 camF = cameraTransform.forward; camF.y = 0; camF.Normalize();
 
            dashDirection = camF;

            isDashing = true;
            dashTimer = dashDuration;

            animator.ResetTrigger("Dash");
            animator.SetTrigger("Dash");
        }
    }

    public void OnDrawEnd()
    {
        Stance = true;
        isBusy = false;
    }
}

