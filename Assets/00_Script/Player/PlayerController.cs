using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("속도 세팅")]
    public float walkSpeed = 3f;
    public float runSpeed = 6;
    public float gravity = -18.81f;
    public float turnSmoothTime = 0.1f;

    [Header("대시 세팅")]
    public KeyCode dashKey = KeyCode.LeftControl;
    public float dashDistance = 3f;      
    public float dashDuration = 0.2f;    
    bool isDashing = false;

    [Header("점프 세팅")]
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpHeight = 2f;
    public int maxJumpCount = 2;
    int jumpCount = 0;

    CharacterController controller;
    Animator animator;
    float turnSmoothVelocity;
    Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.IsTag("Attack"))
        {
            // 중력은 계속 먹여주고
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        if (isDashing) return;

        if (controller.isGrounded)
        {
            if (velocity.y < 0f)
                velocity.y = -2f;      
            jumpCount = 0;             
            animator.SetBool("isGrounded", true);
        }
        else
        {
            animator.SetBool("isGrounded", false);
        }

        if (Input.GetKeyDown(jumpKey) && jumpCount < maxJumpCount)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCount++;

            if (jumpCount == 1) animator.SetTrigger("Jump");
            else animator.SetTrigger("DoubleJump");
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        // 2) Dash 트리거
        if (inputDir.magnitude >= 0.1f
            && Input.GetKeyDown(dashKey))
        {
            StartCoroutine(DoDash(inputDir));
            return;
        }

        bool isWalking = inputDir.magnitude >= 0.1f;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (isWalking)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetAngle,
                ref turnSmoothVelocity, turnSmoothTime
            );
            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
/*        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;*/
    }

    IEnumerator DoDash(Vector3 inputDir)
    {
        isDashing = true;
        animator.SetTrigger("Dash");

        yield return new WaitForSeconds(0.2f);

        Vector3 dashDir = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0)
                          * new Vector3(inputDir.x, 0, inputDir.z);

        float elapsed = 0f;
        float dashSpeed = dashDistance / dashDuration;

        while (elapsed < dashDuration)
        {
            controller.Move(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }
}
