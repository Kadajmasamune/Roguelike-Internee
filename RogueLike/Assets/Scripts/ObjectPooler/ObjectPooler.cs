using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private ObjectManager _objectManager;

    private Dictionary<GameObject, Queue<GameObject>> _pools;

    void Start()
    {
        _objectManager = FindFirstObjectByType<ObjectManager>();
        _pools = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (GameObject prefab in _objectManager._gameObjectsPrefabs)
        {
            _pools[prefab] = new Queue<GameObject>();
        }
    }

 
    public GameObject GetObject(GameObject prefab)
    {
        if (!_pools.ContainsKey(prefab))
        {
            Debug.LogError($"[ObjectPooler] Prefab {prefab.name} is not registered!");
            return null;
        }

        GameObject obj;

        if (_pools[prefab].Count > 0)
        {
            obj = _pools[prefab].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab);
        }

        return obj;
    }

    public void ReturnObject(GameObject prefab, GameObject obj)
    {
        if (!_pools.ContainsKey(prefab))
        {
            Debug.LogError($"[ObjectPooler] Prefab {prefab.name} is not registered");
            Destroy(obj); 
            return;
        }

        obj.SetActive(false);
        _pools[prefab].Enqueue(obj);
    }
}

