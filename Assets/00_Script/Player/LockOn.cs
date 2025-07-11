using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    [Header("락온 설정")]
    public KeyCode lockOnKey = KeyCode.Tab;
    public float lockOnRange = 15f;

    [Header("카메라 설정")]
    public CinemachineCamera playerCamera;

    [Header("플레이어 회전 설정")]
    public float rotationSpeed = 10f;

    public Transform cameraLookTarget;

    private Transform currentTarget;
    private bool isLockedOn = false;

    void Start()
    {
        // 중간점 오브젝트가 만들기
        if (cameraLookTarget == null)
        {
            GameObject lookTargetObj = new GameObject("##CameraLookAtPoint");
            cameraLookTarget = lookTargetObj.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(lockOnKey))
        {
            if (isLockedOn)
            {
                DisableLockOn();
            }
            else
            {
                FindTarget();
            }
        }

        if (isLockedOn && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.position);
            if (distance > lockOnRange)
            {
                DisableLockOn();
            }

            // 락온중에는 플레이어가 타겟을 바라보도록 회전
            RotateTowardsTarget();

            // 카메라 중간점 업데이트
            UpdateMidPoint();
        }
    }

    void UpdateMidPoint()
    {
        // 플레이어 위치 + 몬스터 위치를 2로 나누면 중간!
        Vector3 midPoint = (transform.position + currentTarget.position) / 2f;

        midPoint.y += 1.5f;

        // 계산한 위치로 중간점 오브젝트 이동
        cameraLookTarget.position = midPoint;

        if (playerCamera != null)
        {
            playerCamera.LookAt = cameraLookTarget;
        }
    }

    void RotateTowardsTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0;  

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }
    }

    void FindTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, lockOnRange);

        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Monster"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = hit.transform;
                }
            }
        }

        if (closestTarget != null)
        {
            currentTarget = closestTarget;
            isLockedOn = true;
            EnableLockOnCamera();
        }
    }

    void EnableLockOnCamera()
    {
        if (playerCamera != null && currentTarget != null)
        {
            //playerCamera.LookAt = currentTarget;
            playerCamera.LookAt = cameraLookTarget;
        }
    }

    void DisableLockOn()
    {
        currentTarget = null;
        isLockedOn = false;

        if (playerCamera != null)
        {
            playerCamera.LookAt = transform;

        }

        Debug.Log("락온 해제됨");
    }

}