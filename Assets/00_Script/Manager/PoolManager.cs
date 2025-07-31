using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 싱글톤 패턴
    public static PoolManager Instance;

    [System.Serializable]
    public class PoolInfo
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize = 10;
    }

    [Header("풀 설정")]
    public List<PoolInfo> poolInfos = new List<PoolInfo>();

    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializePools();
    }

    void InitializePools()
    {
        foreach (PoolInfo info in poolInfos)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < info.poolSize; i++)
            {
                GameObject obj = Instantiate(info.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(info.poolName, objectPool);
            Debug.Log($"{info.poolName} 풀 생성 완료: {info.poolSize}개");
        }
    }

    // 범용 Get 메서드
    public GameObject GetObject(string poolName)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"풀을 찾을 수 없습니다: {poolName}");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[poolName];

        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 풀이 비었으면 새로 생성
            PoolInfo info = poolInfos.Find(x => x.poolName == poolName);
            if (info != null)
            {
                GameObject obj = Instantiate(info.prefab);
                Debug.Log($"{poolName} 풀이 비어서 새로 생성");
                return obj;
            }
        }

        return null;
    }

    // 범용 Return 메서드
    public void ReturnObject(string poolName, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"풀을 찾을 수 없습니다: {poolName}");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;

        poolDictionary[poolName].Enqueue(obj);
    }

    // 기존 화살 메서드들 (호환성 유지)
    public GameObject GetArrow()
    {
        return GetObject("Arrow");
    }

    public void ReturnArrow(GameObject arrow)
    {
        ReturnObject("Arrow", arrow);
    }

    // 보스 공격용 메서드들 추가
    public GameObject GetFireBreathProjectile()
    {
        return GetObject("FireBreathProjectile");
    }

    public void ReturnFireBreathProjectile(GameObject projectile)
    {
        ReturnObject("FireBreathProjectile", projectile);
    }

    public GameObject GetFireArea()
    {
        return GetObject("FireArea");
    }

    public void ReturnFireArea(GameObject fireArea)
    {
        ReturnObject("FireArea", fireArea);
    }
}