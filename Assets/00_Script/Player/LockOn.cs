using UnityEngine;

public class LockOn : MonoBehaviour
{
    [Header("¶ô¿Â ¼³Á¤")]
    public KeyCode lockOnKey = KeyCode.Tab;
    public float lockOnRange = 15f;

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

        // Å¸°Ù ¼³Á¤
        if (closestTarget != null)
        {
            currentTarget = closestTarget;
            isLockedOn = true;
            Debug.Log($"Å¸°Ù ¼³Á¤µÊ: {currentTarget.name}");
        }
    }

    void DisableLockOn()
    {
        currentTarget = null;
        isLockedOn = false;
        Debug.Log("¶ô¿Â ÇØÁ¦µÊ");
    }
}
