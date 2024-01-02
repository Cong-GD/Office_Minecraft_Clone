using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace CongTDev.Collection
{
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

            if (capacity == 0)
                return;

            _capacity = capacity;
            _buffer = Allocate(capacity);
        }

        public MyNativeList(ReadOnlySpan<T> span)
        {
            AddRange(span);
        }

        public MyNativeList(MyNativeList<T> list) : this(list.AsSpan())
        {
        }

        public MyNativeList(NativeArray<T> array) : this(array.AsSpan())
        {
        }

        public MyNativeList(IEnumerable<T> collection)
        {
            AddRange(collection);
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
            _buffer[_count++] = item;
        }

        public void AddRange(ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
                return;

            EnsureCapacity(_count + span.Length);
            span.CopyTo(new Span<T>(_buffer + _count, span.Length));
            _count += span.Length;
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (T item in collection)
            {
                Add(item);
            }
        }

        public void AddRange(List<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            EnsureCapacity(_count + list.Count);
            for (int i = 0; i < list.Count; ++i)
            {
                _buffer[_count + i] = list[i];
            }
            _count += list.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0180:Use tuple to swap values", Justification = "<Pending>")]
        public void Reverse()
        {
            if (_count == 0)
                return;

            T* left = _buffer;
            T* right = _buffer + _count - 1;
            while (left < right)
            {
                T temp = *left;
                *left = *right;
                *right = temp;
                ++left;
                --right;
            }
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

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            AsSpan().CopyTo(array.AsSpan(arrayIndex));
        }

        public Span<T> AsSpan()
        {
            return new Span<T>(_buffer, _count);
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
            if (_count == 0)
                return -1;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _count; ++i)
            {
                if (comparer.Equals(item, _buffer[i]))
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

        public struct Enumerator : IEnumerator<T>
        {
            private readonly MyNativeList<T> _list;
            private int _index;

            public Enumerator(MyNativeList<T> list)
            {
                _list = list;
                _index = -1;
            }

            public readonly T Current => _list[_index];

            readonly object IEnumerator.Current => Current;

            public readonly void Dispose()
            {
            }

            public bool MoveNext()
            {
                return ++_index < _list.Count;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}