using UnityEngine;

public class Potion : MonoBehaviour
{
    public Item potion;

    void OnTriggerEnter(Collider other)
    {
        var movement = other.GetComponent<SamuraiMovement>();
        if (movement != null && movement.isMine)
        {
            if (NetworkManager.Instance != null && potion != null)
            {
                NetworkManager.Instance.SendPickupItemPacket(potion.itemId);
            }

            Destroy(gameObject);
        }
    }
}
