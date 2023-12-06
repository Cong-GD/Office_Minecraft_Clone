using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObjectPooling
{
    [CreateAssetMenu(menuName = "ObjectPool")]
    public class ObjectPool : ScriptableObject
    {
        private static Transform _poolParent;
        private static Transform PoolParent
        {
            get
            {
                if (_poolParent == null)
                {
                    _poolParent = new GameObject("Object Pool Parent").transform;
                    _poolParent.gameObject.SetActive(false);
                    DontDestroyOnLoad(_poolParent.gameObject);
                }
                return _poolParent;
            }
        }

        [SerializeField, Required] 
        private Prefab prefab;

        [ShowNonSerializedField]
        private int _currentID = 0;

        private readonly Dictionary<int, Prefab> _pool = new();
        private readonly HashSet<int> _inactive = new();
        private readonly HashSet<int> _active = new();

        [ShowNativeProperty]
        public int CountAll => _pool.Count;

        [ShowNativeProperty]
        public int CountActive => _active.Count;

        [ShowNativeProperty]
        public int CountInactive => _inactive.Count;


        public IPoolObject Get()
        {
            int id;
            if (_inactive.Any())
            {
                id = _inactive.First();
                _inactive.Remove(id);
            }
            else
            {
                id = InstanciateInstance().ID;
            }
            _active.Add(id);
            var instance = _pool[id].Instance;
            instance.transform.SetParent(null);
            return instance;
        }

        private Prefab InstanciateInstance()
        {
            var prefabInstance = Instantiate(prefab);
            prefabInstance.Init(_currentID++, Release, Remove);
            _pool[prefabInstance.ID] = prefabInstance;
            return prefabInstance;
        }

        private void Release(int id)
        {
            var prefabInstance = _pool[id];
            prefabInstance.transform.SetParent(PoolParent);
            _active.Remove(id);
            _inactive.Add(id);
        }

        private void Remove(int id)
        {
            _active.Remove(id);
            _inactive.Remove(id);
            _pool.Remove(id);
        }
    }
}