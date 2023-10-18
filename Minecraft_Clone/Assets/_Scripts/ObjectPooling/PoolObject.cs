using System;
using UnityEngine;

namespace ObjectPooling
{
    public class PoolObject : MonoBehaviour, IPoolObject
    {
        public event Action OnReturn;

        public void ReturnToPool()
        {
            OnReturn?.Invoke();
        }
    }
}