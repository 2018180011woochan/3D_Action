using UnityEngine;

public class BowCombat : MonoBehaviour
{
    public Transform arrowSpawnPoint;   

    public float minForce = 10f;        
    public float maxForce = 300f;        

    private bool isCharging = false;
    private float chargeTime = 0f;
    public float maxChargeTime = 1f;

    void Update()
    {
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

        }

        if (Input.GetMouseButtonUp(1) && isCharging)
        {
            FireArrow();

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

    public void OnWeaponEquipped()
    {
    }

    public void OnWeaponUnequipped()
    {
    }
}
