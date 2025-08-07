using UnityEngine;

public class SamuraiMovement : MonoBehaviour
{
    [Header("상태 변수")]
    public bool Stance = false;
    private bool isBusy = false;    // 현재 플레이어가 다른 작업 중인지

    [Header("상태 변수")]
    public float moveSpeed = 2.0f;
    public float combatMoveSpeed = 2.0f;
    public float rotationSpeed = 10.0f;
    public float gravity = -9.81f;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private float gravityValue = -9.81f;

    [Header("컴포넌트")]
    private Animator animator;
    private CharacterController controller;
    public Transform cameraTransform;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        ApplyGravity();

        if (isBusy) return;

        if (Stance == false)
        {
            HandleMove();
            HandleDraw();
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
        controller.Move(playerVelocity * Time.deltaTime);
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

        animator.SetFloat("Speed", moveDirection.magnitude);
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

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

        animator.SetFloat("Speed", moveDirection.magnitude);

        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

        animator.SetFloat("MoveX", localMoveDirection.x);
        animator.SetFloat("MoveY", localMoveDirection.z);

        controller.Move(moveDirection * combatMoveSpeed * Time.deltaTime);
    }

    private void HandleDraw()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isBusy = true; 
            animator.SetTrigger("Draw"); 
        }
    }

    public void OnDrawEnd()
    {
        Stance = true;  
        isBusy = false; 
    }
}
