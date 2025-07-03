using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;      

    [Header("플레이어 이동 관련")]
    public float moveSpeed = 6f;         
    public float gravity = -9.81f;     
    public float turnSmoothTime = 0.1f;    
    public float jumpHeight = 1.5f;

    CharacterController controller;
    Animator animator;
    float turnSmoothVelocity;
    Vector3 velocity;

    private bool isWalking = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 입력 받아 방향 벡터 만들기
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        isWalking = inputDir.magnitude >= 0.1f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            // 카메라 시야를 기준으로 이동 방향 회전 각도 계산
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;

            // 부드럽게 회전
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 실제 움직일 방향(회전 적용된 전진 방향)
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f)
                                * Vector3.forward;

            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }
        /*
                if (!controller.isGrounded)
                    velocity.y += gravity * Time.deltaTime;
                else
                    velocity.y = -2f;  

                controller.Move(velocity * Time.deltaTime);*/

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 3) 땅에 붙었으면 음수 속도만큼만 고정(충돌감지용)
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;     // skinWidth 이상 파고들도록 충분히 큰 음수
    }
}
