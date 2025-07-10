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

    private Transform currentTarget;
    private bool isLockedOn = false;

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
            playerCamera.LookAt = currentTarget;

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