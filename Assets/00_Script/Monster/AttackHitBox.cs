using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    public float damage = 20f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerState>()?.TakeDamage(damage);
            Debug.Log("�÷��̾� ���� ����");
        }
    }
}
