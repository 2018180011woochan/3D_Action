using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("속도 세팅")]
    public float walkSpeed = 3f;
    public float runSpeed = 6;
    public float gravity = -9.81f;
    public float turnSmoothTime = 0.1f;

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
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

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
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }
}
