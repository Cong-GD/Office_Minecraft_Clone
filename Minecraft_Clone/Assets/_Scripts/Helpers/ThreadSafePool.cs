using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ThreadSafePool<T> where T : class, new()
{
    private const int MAX_POOL_SIZE = 50000;

    private readonly static Queue<T> _pool = new();

    private readonly static object _lockObj = new object();

    public static T Get()
    {
        lock (_lockObj)
        {
            if (_pool.TryDequeue(out var instance))
                return instance;
        }

        return new T();
    }

    public static void Release(T instance)
    {
        lock (_lockObj)
        {
            _pool.Enqueue(instance);
            if (_pool.Count > MAX_POOL_SIZE)
                _pool.Clear();
        }
    }
}
