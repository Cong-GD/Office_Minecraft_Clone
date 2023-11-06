using System;

/// <summary>
/// I made this class just to deal with mesh data, it's not a collection
/// </summary>
public class ArrayBuffer<T> where T : struct
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
}