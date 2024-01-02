using UnityEngine;

using InstanceID = System.Collections.Generic.LinkedListNode<ObjectPooling.Prefab>;

namespace ObjectPooling
{
    [RequireComponent(typeof(IPoolObject))]
    public class Prefab : MonoBehaviour
    {
        public IPoolObject Instance { get; private set; }

        public InstanceID ID { get; private set; }

        public ObjectPool Pool { get; private set; }

        public void Init(InstanceID id, ObjectPool pool)
        {
            ID = id;
            Instance = GetComponent<IPoolObject>();
            Pool = pool;
            Instance.OnReturn += ReturnToPool;
        }

        public void ReturnToPool()
        {
            Pool.Release(ID);
        }

        private void OnDestroy()
        {
            ID?.List?.Remove(ID);
        }
    }
}