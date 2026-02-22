using System.Collections;
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

    [Header("사운드")]
    public AudioClip runLoopSfx;
    public float runVolume = 0.8f;
    private AudioSource runSrc;

    public AudioClip jumpSfx;
    public AudioClip drawSfx;
    [Range(0f, 1f)] public float jumpVolume = 1f;
    private AudioSource sfxSrc;

    private PlayerState playerState;

    private float syncTimer = 0f;
    private float syncInterval = 0.1f; // 0.1초마다 1번씩 전송 (초당 10번)
    public bool isMine = false; // 현재 캐릭터가 본인 클라이언트의 캐릭터인가 

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        playerState = GetComponent<PlayerState>();
        if (runLoopSfx)
        {
            runSrc = gameObject.AddComponent<AudioSource>();
            runSrc.clip = runLoopSfx;
            runSrc.playOnAwake = false;
            runSrc.loop = true;        
            runSrc.spatialBlend = 0f;  
            runSrc.volume = runVolume;
        }

        sfxSrc = gameObject.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        sfxSrc.loop = false;
        sfxSrc.spatialBlend = 0f;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (!isMine) return;
        ApplyGravity();
        HandleDash();

        animator.SetBool("IsStance", Stance);

        bool isJumpingNow = !controller.isGrounded;

        if (isBusy)
        {
            animator.SetBool("Run", false);
            animator.SetFloat("Speed", 0f);

            if (!isDashing && !isJumpingNow)
                playerState?.RecoverStamina(5f * Time.deltaTime);
            return;
        }

        bool shiftHeld = Input.GetKey(KeyCode.LeftShift);
        bool hasStaminaToRun = (playerState == null) || (playerState.currentStamina > 0f);
        isRunning = shiftHeld && hasStaminaToRun;

        HandleJump();
        HandleDraw();

        if (!Stance) HandleMove();
        else HandleCombatMove();

        UpdateRunSfx();

        if (!isRunning && !isDashing && !isJumpingNow)
            playerState?.RecoverStamina(10f * Time.deltaTime);

        SyncTransformToServer();
    }

    private void SyncTransformToServer()
    {
        syncTimer += Time.deltaTime;

        if (syncTimer >= syncInterval)
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.SendMovePacket(transform.position, transform.eulerAngles.y);
            }

            syncTimer = 0f;
        }
    }

    void UpdateRunSfx()
    {
        if (!runSrc) return;

        // 가로 속도만 사용
        Vector3 hv = controller.velocity; hv.y = 0f;
        float speed = hv.magnitude;

        bool shouldPlay =
            isGrounded &&            
            !isBusy &&               
            !isDashing &&            
            isRunning &&             
            speed > 0.1f;            

        if (shouldPlay)
        {
            if (!runSrc.isPlaying) runSrc.Play();
            runSrc.pitch = Mathf.Lerp(0.95f, 1.05f, Mathf.InverseLerp(moveSpeed, runSpeed, speed));
            playerState?.ConsumeStamina(5f * Time.deltaTime);
        }
        else
        {
            if (runSrc.isPlaying) runSrc.Stop();
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
        float currentSpeed = canRun ? runSpeed : combatMoveSpeed;

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
    private Coroutine drawSfxCo;
    private IEnumerator PlayDrawSfxDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        if (drawSfx) sfxSrc.PlayOneShot(drawSfx, 1f);
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
                    StartCoroutine(PlayDrawSfxDelayed());
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
        if (Input.GetKeyDown(KeyCode.Space)
            && isGrounded
            && (playerState == null || playerState.currentStamina >= 10f)) 
        {
            animator.SetTrigger("Jump");
            if (jumpSfx) sfxSrc.PlayOneShot(jumpSfx, jumpVolume);
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);

            playerState?.ConsumeStamina(10f);
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

        bool hasStaminaToDash = (playerState == null) || (playerState.currentStamina >= 10f); 

        if (Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0f && !isBusy && v > 0.1f && hasStaminaToDash)
        {
            Vector3 camF = cameraTransform.forward; camF.y = 0; camF.Normalize();
            dashDirection = camF;

            isDashing = true;
            if (jumpSfx) sfxSrc.PlayOneShot(jumpSfx, jumpVolume);
            dashTimer = dashDuration;

            animator.ResetTrigger("Dash");
            animator.SetTrigger("Dash");

            playerState?.ConsumeStamina(10f); 
        }
    }

    public void OnDrawEnd()
    {
        Stance = true;
        isBusy = false;
    }
}

