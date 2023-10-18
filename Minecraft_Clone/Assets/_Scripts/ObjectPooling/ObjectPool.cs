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
        [SerializeField] private Prefab prefab;

        private int _currentID = 0;

        private readonly Dictionary<int, Prefab> _pool = new();
        private readonly HashSet<int> _inactive = new();
        private readonly HashSet<int> _active = new();

        public IPoolObject Get()
        {
            int id;
            if(_inactive.Any())
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

        public bool Get<T>(out T instance) where T : IPoolObject
        {
            try
            {
                instance = (T)Get();
                return true;
            }
            catch
            {
                Debug.LogWarning($"Error when try to get object of type {typeof(T)} from pool");
            }
            instance = default;
            return false;
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