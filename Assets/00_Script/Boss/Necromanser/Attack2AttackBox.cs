using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[DisallowMultipleComponent]
public class AttackHitInstantOnce : MonoBehaviour
{
    public int criticalDamage = 20;
    public float lifeTime = 5f;   

    BoxCollider col;
    bool done;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Start()
    {
        if (lifeTime > 0f) Destroy(gameObject, lifeTime);
        InstantHit();   
    }


    void InstantHit()
    {
        if (done) return;

        Vector3 center = transform.TransformPoint(col.center);
        Vector3 half = 0.5f * Vector3.Scale(col.size, transform.lossyScale);

        var cols = Physics.OverlapBox(center, half, transform.rotation, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            if (!c || !c.CompareTag("Player")) continue;

            var ps = c.GetComponentInParent<PlayerState>();
            if (ps) {
                ps.TakeCriticalDamage(criticalDamage); 
                Finish();
            }
            break;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (done || !other.CompareTag("Player")) return;

        var ps = other.GetComponentInParent<PlayerState>();
        if (ps) { ps.TakeCriticalDamage(criticalDamage); Finish(); }
    }

    void Finish()
    {
        done = true;
        if (col) col.enabled = false; 
        Destroy(this);                  
    }
}
