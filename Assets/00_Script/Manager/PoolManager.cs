using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static PoolManager Instance;

    [Header("화살 풀 설정")]
    public GameObject arrowPrefab;
    public int poolSize = 20; 

    private Queue<GameObject> arrowPool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab);
            arrow.SetActive(false);  
            arrowPool.Enqueue(arrow);  
        }

        Debug.Log($"화살 풀 생성 완료: {poolSize}개");
    }

    public GameObject GetArrow()
    {
        if (arrowPool.Count > 0)
        {
            GameObject arrow = arrowPool.Dequeue();
            arrow.SetActive(true);
            return arrow;
        }
        else
        {
            // 풀이 비었으면 새로 생성
            GameObject arrow = Instantiate(arrowPrefab);
            return arrow;
        }
    }

    public void ReturnArrow(GameObject arrow)
    {
        arrow.SetActive(false);
        arrow.transform.position = Vector3.zero;  // 위치 초기화
        arrow.transform.rotation = Quaternion.identity;  // 회전 초기화
        arrowPool.Enqueue(arrow);
    }
}
