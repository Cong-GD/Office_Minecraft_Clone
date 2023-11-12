using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// I made this class just to deal with mesh data, it's not a collection
/// </summary>
public class ArrayBuffer<T> : IEnumerable<T> where T : struct
{
    private T[] _items;

    private int _count;

    public T[] Items => _items;

    public int Count => _count;

    public int Capacity => _items.Length;

    public ArrayBuffer(int capacity = 4)
    {
        _items = new T[capacity];
        _count = 0;
    }

    public void Add(T item)
    {
        if (_count >= _items.Length)
        {
            Array.Resize(ref _items, _items.Length * 2);
        }
        _items[_count] = item;
        _count++;
    }

    public void Clear()
    {
        _count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
    {
        private readonly T[] _items;
        private int _index;
        private readonly int _count;

        public Enumerator(ArrayBuffer<T> buffer)
        {
            _items = buffer._items;
            _count = buffer._count;
            _index = -1;
        }

        public T Current => _items[_index];

        object IEnumerator.Current => _items[_index];

        public void Dispose() { }

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}