using UnityEngine;

public class BowCombat : MonoBehaviour
{
    public Transform arrowSpawnPoint;   

    public float minForce = 10f;        
    public float maxForce = 30f;        

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
        Vector3 fireDirection = transform.forward;

        // 화살이 발사 방향을 바라보도록 회전
        arrow.transform.rotation = Quaternion.LookRotation(fireDirection) * Quaternion.Euler(90, 0, 0);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * fireForce;

    }

    public void OnWeaponEquipped()
    {
    }

    public void OnWeaponUnequipped()
    {
    }
}
