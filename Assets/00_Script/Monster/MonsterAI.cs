using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("�÷��̾� Ž�� �ݰ�")]
    public float detectionRange = 5f;

    [Header("Ž���� Ÿ�� �÷��̾�")]
    public Transform playerTransform;

    void Reset()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }
}
