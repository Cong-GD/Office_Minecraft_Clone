using System;
using UnityEngine;

namespace ObjectPooling
{
    public class PoolObject : MonoBehaviour, IPoolObject
    {
        public event Action OnReturn;

        public void ReturnToPool()
        {
            if(OnReturn != null) 
                OnReturn();
            else
                Destroy(gameObject);
        }
    }
}