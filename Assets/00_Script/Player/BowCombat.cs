using UnityEngine;

public class BowCombat : MonoBehaviour
{
    public Transform arrowSpawnPoint;   

    public float minForce = 10f;        
    public float maxForce = 300f;        

    private bool isCharging = false;
    private float chargeTime = 0f;
    public float maxChargeTime = 1f;

    private LockOn lockOn;

    private void Start()
    {
        lockOn = GetComponent<LockOn>();
    }
    void Update()
    {
        if (lockOn.isLockedOn == false)
            CrosshairManager.Instance.ShowCrosshair(false);
        else
            CrosshairManager.Instance.ShowCrosshair(true);

        if (Input.GetMouseButtonDown(1))
        {
            isCharging = true;
            chargeTime = 0f;
        }

        if (isCharging)
        {
            chargeTime += Time.deltaTime;
            if (chargeTime > maxChargeTime)
            {
                chargeTime = maxChargeTime;
            }
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
        }
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
            // 부드럽게 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public void OnWeaponEquipped()
    {
        isCharging = false;
        chargeTime = 0f;
        //CrosshairManager.Instance.ShowCrosshair(true);
    }

    public void OnWeaponUnequipped()
    {
        if (isCharging)
        {
            isCharging = false;
            chargeTime = 0f;
        }
        CrosshairManager.Instance.ShowCrosshair(false);
    }
}
