using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

using InstanceID = System.Collections.Generic.LinkedListNode<ObjectPooling.Prefab>;

namespace ObjectPooling
{
    [CreateAssetMenu(menuName = "ObjectPool")]
    public class ObjectPool : ScriptableObject
    {
        [SerializeField, Required]
        private Prefab prefab;

        private readonly LinkedList<Prefab> _inactive = new();
        private readonly LinkedList<Prefab> _active = new();

        [ShowNativeProperty]
        public int CountAll => _active.Count + _inactive.Count;

        [ShowNativeProperty]
        public int CountActive => _active.Count;

        [ShowNativeProperty]
        public int CountInactive => _inactive.Count;


        public IPoolObject Get(Transform parent = null)
        {
            InstanceID id;
            if (_inactive.Count == 0)
            {
                InstanciateInstance();
            }
            id = _inactive.Last;
            _inactive.RemoveLast();
            _active.AddLast(id);
            IPoolObject instance = id.Value.Instance;
            instance.gameObject.SetActive(true);
            instance.transform.SetParent(parent);
            return instance;
        }

        private void InstanciateInstance()
        {
            Prefab prefabInstance = Instantiate(prefab);
            InstanceID id = _inactive.AddLast(prefabInstance);
            prefabInstance.Init(id, this);
            prefabInstance.gameObject.SetActive(false);
        }

        public void Release(InstanceID id)
        {
            id.Value.gameObject.SetActive(false);
            _active.Remove(id);
            _inactive.AddFirst(id);
        }
    }
}