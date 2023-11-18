using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public unsafe class MyNativeList<T> : IDisposable, ICollection<T>, IEnumerable<T> where T : unmanaged
{
    private const int DEFAULT_CAPACITY = 4;
    private static readonly int _maxSize = int.MaxValue / sizeof(T);

    private T* _buffer = null;

    private int _count = 0;

    private int _capacity = 0;

    public int Count => _count;

    public int Capacity => _capacity;

    public bool IsReadOnly => false;

    public MyNativeList()
    {

    }

    public MyNativeList(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException("Capacity can't less than 0");

        _capacity = capacity;
        _buffer = Allocate(capacity);
    }

    ~MyNativeList()
    {
        FreeMemory();
    }

    public NativeArray<T> AsNativeArray()
    {
        var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(_buffer, _count, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
        return nativeArray;
    }

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= _count)
                throw new IndexOutOfRangeException();

            return ref *(_buffer + index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_count == _capacity)
        {
            EnsureCapacity(_count + 1);
        }
        _buffer[_count] = item;
        _count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _count = 0;
    }

    private void EnsureCapacity(int minSize)
    {
        if (_capacity <= minSize)
        {
            int newCapacity = _capacity == 0 ? DEFAULT_CAPACITY : _capacity * 2;
            if ((uint)newCapacity > _maxSize) newCapacity = _maxSize;
            if (newCapacity < minSize) newCapacity = minSize;

            T* newBuffer = Allocate(newCapacity);
            UnsafeUtility.MemCpy(newBuffer, _buffer, _count * sizeof(T));
            FreeMemory();
            _capacity = newCapacity;
            _buffer = newBuffer;
        }
    }

    private static T* Allocate(int length)
    {
        if (length < 1)
            return null;

        return (T*)Marshal.AllocHGlobal(sizeof(T) * length);
    }

    private void FreeMemory()
    {
        if (_buffer != null)
        {
            Marshal.FreeHGlobal((IntPtr)_buffer);
            _buffer = null;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (_buffer[i].Equals(item))
                return true;
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException("array");

        int end = Mathf.Min(array.Length, _count);
        for (int i = arrayIndex, j = 0; i < end && j < end; ++i, ++j)
        {
            array[i] = _buffer[j];
        }
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index == -1)
            return false;
        RemoveAt(index);
        return true;
    }

    public int IndexOf(T item)
    {
        for (int i = 0; i < _count; ++i)
        {
            if (item.Equals(_buffer[i]))
                return i;
        }
        return -1;
    }

    public void Insert(int index, T item)
    {
        if ((uint)index >= _count)
            throw new IndexOutOfRangeException();

        if (_count == _capacity)
            EnsureCapacity(_capacity + 1);

        for (int i = _count; i > index; --i)
        {
            _buffer[i] = _buffer[i - 1];
        }
        _buffer[index] = item;
        ++_count;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= _count)
            throw new IndexOutOfRangeException();

        int end = _count - 1;
        for (int i = index; i < end; i++)
        {
            _buffer[i] = _buffer[i + 1];
        }
        --_count;
    }

    public void Dispose()
    {
        FreeMemory();
        _count = 0;
        _capacity = 0;
    }

    public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
    {
        private readonly T* _buffer;
        private int _index;
        private readonly int _count;
        private T _value;

        public Enumerator(MyNativeList<T> buffer)
        {
            _buffer = buffer._buffer;
            _count = buffer._count;
            _index = -1;
            _value = default;
        }

        public readonly T Current => _value;

        readonly object IEnumerator.Current => _value;

#pragma warning disable IDE0251 // Make member 'readonly'
        public void Dispose() { }
#pragma warning restore IDE0251 // Make member 'readonly'

        public bool MoveNext()
        {
            if (_index >= _count - 1)
            {
                return false;
            }
            _index++;
            _value = _buffer[_index];
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}