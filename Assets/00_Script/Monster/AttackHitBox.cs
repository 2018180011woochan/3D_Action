using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    public float damage = 10f;
    Animator animator;
    public GameObject AttackEffect;

    bool hasSwing = false;

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    void OnEnable()
    {
        hasSwing = false;
    }

    void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);   // 겹쳐있는 프레임에서도 판정
    }
    public void ResetSwing() => hasSwing = false;
    void OnTriggerEnter(Collider other)
    {
        if (hasSwing) return;

        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return;

        var ps = other.GetComponentInParent<PlayerState>();
        var movement = other.GetComponentInParent<SamuraiMovement>();

        if (ps == null || movement == null) return;
        if (!movement.isMine) return;

        hasSwing = true;

        bool isBlocked = ps.IsBlocking();

        var ms = GetComponentInParent<MonsterState>();
        int mobId = ms != null ? ms.monsterId : -1;

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SendHitPlayerPacket(mobId, damage, isBlocked);
        }

        if (isBlocked)
        {
            animator.SetTrigger("GetHit"); 
        }

        if (AttackEffect != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Instantiate(AttackEffect, hitPoint, Quaternion.identity);
        }
    }
}