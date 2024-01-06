using System;
using UnityEngine;

public static class ThreadSafePool<T> where T : class, new()
{
    private const int DEFAULT_CAPACITY = 2;

    public static int Capacity
    {
        get => _pool.Length;
        set
        {
            Array.Resize(ref _pool, value);
        }
    }

    public static int Count { get; private set; }

    private static T[] _pool = new T[DEFAULT_CAPACITY];

    private readonly static object _lockObj = new object();

    public static T Get()
    {
        lock (_lockObj)
        {
            if (Count > 0)
            {
                --Count;
                T instance = _pool[Count];
                _pool[Count] = null;
                return instance;
            }
        }

        return new T();
    }

    public static void Release(T instance)
    {
        lock (_lockObj)
        {
            if (Count < _pool.Length)
            {
                _pool[Count++] = instance;
            }
        }
    }
}
