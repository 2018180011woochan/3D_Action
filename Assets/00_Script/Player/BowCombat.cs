using UnityEngine;

public class BowCombat : MonoBehaviour
{
    [Header("화살 설정")]
    public GameObject arrowPrefab;      // 화살 프리팹
    public Transform arrowSpawnPoint;   // 화살 생성 위치

    [Header("발사 힘")]
    public float minForce = 10f;        // 최소 발사 힘
    public float maxForce = 30f;        // 최대 발사 힘

    private bool isCharging = false;
    private float chargeTime = 0f;
    public float maxChargeTime = 1f;

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("활 당기기 시작!");
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
        // 차징 비율 계산
        float chargePercent = chargeTime / maxChargeTime;

        // 발사 힘 계산
        float fireForce = Mathf.Lerp(minForce, maxForce, chargePercent);

        Debug.Log($"발사 힘: {fireForce}");
    }

    public void OnWeaponEquipped()
    {
        // 활 장착 시 처리
    }

    public void OnWeaponUnequipped()
    {
        // 활 해제 시 처리
    }
}
