using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform cameraTransform;      

    [Header("�÷��̾� �̵� ����")]
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
        // �Է� �޾� ���� ���� �����
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        isWalking = inputDir.magnitude >= 0.1f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            // ī�޶� �þ߸� �������� �̵� ���� ȸ�� ���� ���
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg
                                + cameraTransform.eulerAngles.y;

            // �ε巴�� ȸ��
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // ���� ������ ����(ȸ�� ����� ���� ����)
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

        // 3) ���� �پ����� ���� �ӵ���ŭ�� ����(�浹������)
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;     // skinWidth �̻� �İ�鵵�� ����� ū ����
    }
}
