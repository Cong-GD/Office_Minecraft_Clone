using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class MyList<T> : IList<T>, ICollection<T>, IEnumerable<T> where T : struct
{
    private const int DEFAULT_CAPACITY = 4;
    private const int MAX_SIZE = 10000000;

    private T[] _items;

    private int _count;

    public T[] Items => _items;

    public int Count => _count;

    public int Capacity => _items.Length;

    public bool IsReadOnly => false;

    public MyList()
    {
        _items = Array.Empty<T>();
    }

    public MyList(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException("Cappacity can't less than 0");

        _items = _count == 0 ? Array.Empty<T>() : new T[capacity];
        _count = 0;
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= _count)
                throw new IndexOutOfRangeException();

            return _items[index];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if ((uint)index >= _count)
                throw new IndexOutOfRangeException();

            _items[index] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_count == _items.Length)
        {
            EnsureCapacity(_items.Length + 1);
        }
        _items[_count] = item;
        _count++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _count = 0;
    }

    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            int newCapacity = _items.Length == 0 ? DEFAULT_CAPACITY : _items.Length * 2;
            if ((uint)newCapacity > MAX_SIZE) newCapacity = MAX_SIZE;
            if (newCapacity < min) newCapacity = min;
            Array.Resize(ref _items, newCapacity);
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
            if (item.Equals(_items[i]))
                return true;
        }
        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Array.Copy(_items, 0, array, arrayIndex, _items.Length);
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
            if (item.Equals(_items[i]))
                return i;
        }
        return -1;
    }

    public void Insert(int index, T item)
    {
        if ((uint)index >= _count)
            throw new IndexOutOfRangeException();

        if (_count == _items.Length)
            EnsureCapacity(_items.Length + 1);

        for (int i = _count; i > index; --i)
        {
            _items[i] = _items[i - 1];
        }
        _items[index] = item;
        ++_count;
    }

    public void RemoveAt(int index)
    {
        if ((uint)index >= _count)
            throw new IndexOutOfRangeException();

        int end = _count - 1;
        for (int i = index; i < end; i++)
        {
            _items[i] = _items[i + 1];
        }
        --_count;
    }

    public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
    {
        private readonly T[] _items;
        private int _index;
        private readonly int _count;

        public Enumerator(MyList<T> buffer)
        {
            _items = buffer._items;
            _count = buffer._count;
            _index = -1;
        }

        public T Current
        {
            get
            {
                if ((uint)_index >= _count)
                    throw new InvalidOperationException();

                return _items[_index];

            }
        }

        object IEnumerator.Current
        {
            get
            {
                if ((uint)_index >= _count)
                    throw new InvalidOperationException();

                return _items[_index];

            }
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            if (_index >= _count - 1)
            {
                return false;
            }
            _index++;
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
