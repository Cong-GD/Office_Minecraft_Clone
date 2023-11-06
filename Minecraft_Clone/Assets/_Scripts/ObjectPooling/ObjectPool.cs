using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObjectPooling
{
    public interface IPoolObject
    {
        GameObject gameObject { get; }

        event Action OnReturn;
        void ReturnToPool();
    }


    [CreateAssetMenu(menuName = "ObjectPool")]
    public class ObjectPool : ScriptableObject
    {
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
            instance.gameObject.SetActive(true);
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
            _pool[id].gameObject.SetActive(false);
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