using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;

public class BowCombat : MonoBehaviour
{
    public Transform arrowSpawnPoint;

    public float minForce = 10f;
    public float maxForce = 300f;

    private bool isCharging = false;
    private float chargeTime = 0f;
    public float maxChargeTime = 1f;

    private LockOn lockOn;
    private PlayerCombat playerCombat;
    private Animator animator;
    private PlayerController playerController;

    public GameObject fireSkillStartEffect;
    public GameObject fireSkillProjectile;
    public bool fireSkill = false;
    private bool fireSkillOnCooldown = false;
    private float fireSkillCoolTime = 20f;
    public TimelineAsset fireSkillTimeline;

    // 움직임 감지를 위한 변수
    private bool isMoving = false;

    private void Start()
    {
        lockOn = GetComponent<LockOn>();
        playerCombat = GetComponent<PlayerCombat>();
        animator = playerCombat.GetAnimator();
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (lockOn.isLockedOn == false)
            CrosshairManager.Instance.ShowCrosshair(false);
        else
            CrosshairManager.Instance.ShowCrosshair(true);

        // 움직임 상태 확인
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        isMoving = (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f);

        if (Input.GetMouseButtonDown(1))
        {
            isCharging = true;
            chargeTime = 0f;

            UpdateChargingAnimation();
        }

        if (isCharging)
        {
            chargeTime += Time.deltaTime;
            if (chargeTime > maxChargeTime)
            {
                chargeTime = maxChargeTime;
            }

            UpdateChargingAnimation();

            RotateTowardsCrosshair();
        }

        if (Input.GetMouseButtonUp(1) && isCharging)
        {
            if (lockOn.isLockedOn == false)
                FireArrow();
            else
                LockOnFireArrow();

            isCharging = false;
            chargeTime = 0f;

            // 차징 종료 시 애니메이션 초기화
            animator.SetBool("isStandCharging", false);
            animator.SetBool("isMoveCharging", false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartFireSkillCutscene();
        }
    }

    void StartFireSkillCutscene()
    {
        if (playerCombat.IsPlayingCutscene) return;
        if (fireSkillOnCooldown) return;

        UIManager.Instance.StartFireSkillCooldown(fireSkillCoolTime);
        playerCombat.IsPlayingCutscene = true;
        fireSkillOnCooldown = true;

        animator.SetTrigger("Skill1");
        ApplyChargingRotationOffset();
        fireSkill = true;

        Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);

        Instantiate(fireSkillStartEffect, transform.position, rot);

        if (playerCombat.playerCamera != null && playerCombat.skillCutsceneCamera != null)
        {
            playerCombat.playerCamera.Priority = 0;
            playerCombat.skillCutsceneCamera.Priority = 10;
        }

        if (playerCombat.skillCutsceneDirector != null && fireSkillTimeline != null)
        {
            playerCombat.skillCutsceneDirector.playableAsset = fireSkillTimeline;
            playerCombat.skillCutsceneDirector.Play();
        }

        StartCoroutine(ShotFireSkill(2f));
        StartCoroutine(FireSkillCooldown(fireSkillCoolTime));
    }

    IEnumerator ShotFireSkill(float time)
    {
        yield return new WaitForSeconds(time);

        // 스킬 발사
        Quaternion rot = Quaternion.LookRotation(transform.forward, Vector3.up);
        rot *= Quaternion.Euler(0, 270f, 0);

        //Vector3 spawnPosition = transform.position + Vector3.up * 2f;
        Instantiate(fireSkillProjectile, transform.position, rot);
    }

    IEnumerator FireSkillCooldown(float cooldownTime)
    {
        yield return new WaitForSeconds(cooldownTime);
        fireSkillOnCooldown = false;
    }

    void UpdateChargingAnimation()
    {
        if (!isCharging) return;

        if (isMoving)
        {
            // 움직이면서 차징
            animator.SetBool("isStandCharging", false);
            animator.SetBool("isMoveCharging", true);
        }
        else
        {
            // 서서 차징
            animator.SetBool("isStandCharging", true);
            animator.SetBool("isMoveCharging", false);
        }
    }

    // 차징 중 회전 보정을 위한 메서드
    void ApplyChargingRotationOffset()
    {
        // 현재 회전값에 90도 추가 회전 적용
        transform.rotation = transform.rotation * Quaternion.Euler(0, 90f, 0);
    }

    void FireArrow()
    {
        float chargePercent = chargeTime / maxChargeTime;
        float fireForce = Mathf.Lerp(minForce, maxForce, chargePercent);

        GameObject arrow = PoolManager.Instance.GetArrow();
        arrow.transform.position = arrowSpawnPoint.position;

        // 차징량에 따라 각도 조절
        float upwardAngle = Mathf.Lerp(20f, 35f, chargePercent);

        // 발사 방향 계산
        Vector3 fireDirection = Quaternion.AngleAxis(-upwardAngle, transform.right) * transform.forward;

        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.Initialize(fireDirection, fireForce);
        }
    }

    void LockOnFireArrow()
    {
        float chargePercent = chargeTime / maxChargeTime;
        float fireForce = Mathf.Lerp(minForce, maxForce, chargePercent);

        GameObject arrow = PoolManager.Instance.GetArrow();
        arrow.transform.position = arrowSpawnPoint.position;

        // 화면 중앙(크로스헤어)에서 Ray 발사
        Vector3 screenCenter = CrosshairManager.Instance.crosshair.transform.position;
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        RaycastHit hit;
        Vector3 targetPoint;

        // 목표 지점 찾기
        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(50f);
        }

        // 화살 발사 방향 계산
        Vector3 fireDirection = (targetPoint - arrowSpawnPoint.position).normalized;

        // 상향 각도 추가 (거리에 따라 조절)
        float distance = Vector3.Distance(arrowSpawnPoint.position, targetPoint);
        float upwardAngle = Mathf.Lerp(5f, 20f, distance / 50f);  // 거리가 멀수록 각도 증가

        // 위쪽으로 회전 적용
        Vector3 right = Vector3.Cross(Vector3.up, fireDirection);
        fireDirection = Quaternion.AngleAxis(-upwardAngle, right) * fireDirection;

        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.Initialize(fireDirection, fireForce);
        }
    }

    void RotateTowardsCrosshair()
    {
        // 크로스헤어 위치에서 Ray 발사
        Ray ray = Camera.main.ScreenPointToRay(CrosshairManager.Instance.crosshair.transform.position);
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(50f);
        }

        // 플레이어가 바라볼 방향 (Y축 회전만)
        Vector3 lookDirection = targetPoint - transform.position;
        lookDirection.y = 0; // 수평 회전만

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

            // 차징 중일 때 90도 추가 회전 적용
            if (isCharging)
            {
                targetRotation *= Quaternion.Euler(0, 90f, 0);
            }

            // 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void OnWeaponEquipped()
    {
        isCharging = false;
        chargeTime = 0f;

        //animator.SetBool("isStandCharging", false);
        //animator.SetBool("isMoveCharging", false);

    }

    public void OnWeaponUnequipped()
    {
        if (isCharging)
        {
            isCharging = false;
            chargeTime = 0f;

            // 애니메이션 상태 초기화
            animator.SetBool("isStandCharging", false);
            animator.SetBool("isMoveCharging", false);
        }
        CrosshairManager.Instance.ShowCrosshair(false);
    }
}