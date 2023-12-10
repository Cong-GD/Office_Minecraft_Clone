using System;
using UnityEngine;

public static class ThreadSafePool<T> where T : class, new()
{

    public const int POOL_SIZE_LIMIT = 2000;
    private const int defaultCapacity = 5;

    public static int Capacity
    {
        get => pool.Length;
        set
        {
            int newSize = Mathf.Clamp(value, 0, POOL_SIZE_LIMIT);
            Array.Resize(ref pool, newSize);
        }
    }

    public static int Count { get; private set; }

    private static T[] pool = new T[defaultCapacity];

    private readonly static object _lockObj = new object();

    public static T Get()
    {
        lock (_lockObj)
        {
            if (Count > 0)
            {
                --Count;
                T instance = pool[Count];
                pool[Count] = null;
                return instance;
            }
        }

        return new T();
    }

    public static void Release(T instance)
    {
        lock (_lockObj)
        {
            if (Count < pool.Length)
            {
                pool[Count++] = instance;
            }
        }
    }
}
