using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float destroyTime = 3f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
