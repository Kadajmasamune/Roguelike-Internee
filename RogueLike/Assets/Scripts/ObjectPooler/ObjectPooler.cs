using UnityEngine;

public unsafe class ObjectPooler : MonoBehaviour
{
    ObjectManager _objectManager;
    private long _HeapAllocatedMem;

    void Start()
    {
        _objectManager = FindFirstObjectByType<ObjectManager>();
    }
    
}
