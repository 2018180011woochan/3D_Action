using UnityEngine;

public class AutoNonActive : MonoBehaviour
{
    public float disableTime = 3f;

    void OnEnable()
    {
        CancelInvoke();                          
        Invoke(nameof(GoInactive), disableTime); 
    }

    void OnDisable()
    {
        CancelInvoke();
    }

    void GoInactive()
    {
        gameObject.SetActive(false);
    }
}
