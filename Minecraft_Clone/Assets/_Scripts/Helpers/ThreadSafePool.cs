using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ThreadSafePool<T> where T : class, new()
{

    public const int POOL_SIZE_LIMIT = 2000;
    private static int _maxPoolSize = 50;

    public static int Capacity
    {
        get { return _maxPoolSize; }
        set 
        { 
            value = Mathf.Clamp(value, 0, POOL_SIZE_LIMIT);
            _maxPoolSize = value; 
        }
    }

    public static int Count => _pool.Count;

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
            if (_pool.Count < _maxPoolSize)
                _pool.Enqueue(instance);
        }
    }
}
