using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("플레이어 탐지 반경")]
    public float detectionRange = 5f;

    [Header("탐지할 타겟 플레이어")]
    public Transform playerTransform;

    void Reset()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }
}
