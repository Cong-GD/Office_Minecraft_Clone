using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace CongTDev.Collection
{
    public class BinaryHeap<T> : ICollection<T>
    {
        private T[] _heap;
        private readonly IComparer<T> _comparer;

        public int Count { get; private set; }

        public int Length => _heap.Length;

        public bool IsReadOnly => false;

        public BinaryHeap() : this(3, null)
        {
        }

        public BinaryHeap(int capacity, IComparer<T> comparer = null)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("Capacity can't less than 0");

            _heap = new T[capacity];
            _comparer = comparer ?? Comparer<T>.Default;
        }

        private void DoubleSize()
        {
            Array.Resize(ref _heap, (int)Math.Min((uint)Length * 2u, int.MaxValue));
        }

        public void Add(T item)
        {
            if (Count == Length)
                DoubleSize();

            _heap[Count] = item;
            Count++;
            HeapifyUp(Count - 1);
        }

        public T Extract()
        {
            if (Count == 0)
                throw new InvalidOperationException("Heap is empty");

            T result = _heap[0];
            RemoveAt(0);
            return result;
        }

        public bool TryExtract(out T result)
        {
            if (Count == 0)
            {
                result = default;
                return false;
            }

            result = _heap[0];
            RemoveAt(0);
            return true;
        }

        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("Heap is empty");

            return _heap[0];
        }

        public bool TryPeek(out T result)
        {
            if (Count == 0)
            {
                result = default;
                return false;
            }

            result = _heap[0];
            return true;
        }

        public void Clear()
        {
            Array.Clear(_heap, 0, Count);
            Count = 0;
        }

        public int IndexOf(T item)
        {
            if (Count == 0)
                return -1;

            if (_comparer.Compare(item, _heap[0]) < 0)
                return -1;

            return Array.IndexOf(_heap, item, 0, Count);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            return RemoveAt(index);
        }

        public bool RemoveAt(int index)
        {
            if ((uint)index >= Count)
                return false;

            --Count;
            _heap[index] = _heap[Count];
            _heap[Count] = default;
            HepifyDown(index);
            return true;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public bool Update(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;

            UpdateAt(index);
            return true;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Index is out of range");

            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Array is too small");

            Array.Copy(_heap, 0, array, arrayIndex, Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int UpdateAt(int index)
        {
            int newIndex = HeapifyUp(index);
            if (newIndex == index)
            {
                newIndex = HepifyDown(newIndex);
            }
            return newIndex;
        }

        private int HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = Parent(index);
                if (!IsValidNode(parent))
                    break;

                if (_comparer.Compare(_heap[index], _heap[parent]) >= 0)
                    break;

                Swap(index, parent);
                index = parent;
            }
            return index;
        }

        private int HepifyDown(int index)
        {
            while (true)
            {
                int smallest = index;
                int left = Left(index);
                int right = Right(index);

                if (IsValidNode(left) && _comparer.Compare(_heap[left], _heap[smallest]) < 0)
                    smallest = left;

                if (IsValidNode(right) && _comparer.Compare(_heap[right], _heap[smallest]) < 0)
                    smallest = right;

                if (smallest == index)
                    break;

                Swap(index, smallest);
                index = smallest;
            }
            return index;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Parent(int index) => (index - 1) / 2;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Left(int index) => 2 * index + 1;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Right(int index) => 2 * index + 2;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidNode(int index) => (uint)index < Count;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Swap(int index1, int index2)
        {
            (_heap[index2], _heap[index1]) = (_heap[index1], _heap[index2]);
        }

#pragma warning disable IDE0251 // Make member 'readonly'
        public struct Enumerator : IEnumerator<T>
        {
            private readonly BinaryHeap<T> _heap;
            private int _index;

            public Enumerator(BinaryHeap<T> heap)
            {
                _heap = heap;
                _index = -1;
            }

            public T Current => _heap._heap[_index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                return ++_index < _heap.Count;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
#pragma warning restore IDE0251 // Make member 'readonly'

    }
}