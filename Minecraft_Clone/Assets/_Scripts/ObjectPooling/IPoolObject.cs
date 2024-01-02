using System;
using UnityEngine;

namespace ObjectPooling
{
    public interface IPoolObject
    {
        GameObject gameObject { get; }

        Transform transform { get; }

        event Action OnReturn;

        void ReturnToPool();
    }
}