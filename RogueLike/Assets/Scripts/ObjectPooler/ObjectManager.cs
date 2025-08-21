using System.Collections.Generic;
using UnityEngine;

public unsafe class ObjectManager : MonoBehaviour
{
    [SerializeField] public List<GameObject> _gameObjectsPrefabs = new List<GameObject>(200);
}
