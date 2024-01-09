using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CongTDev.Collection
{
    public unsafe class ByteString : IDisposable
    {
        private const int DEFAULT_CAPACITY = 4;

        private const int MAX_SIZE = int.MaxValue;

        private const int MEMORY_THRESHOLD = 10_000;

        private byte* _buffer = null;

        private int _count = 0;

        private int _capacity = 0;

        public int Count => _count;

        public int Capacity => _capacity;

        public bool IsReadOnly => false;

        public bool IsEmpty => _count == 0;

        static ByteString()
        {
            ThreadSafePool<ByteString>.Capacity = 10;
        }

        public static ByteString Create(int capacity = 0)
        {
            ByteString byteString = ThreadSafePool<ByteString>.Get();
            byteString.Clear();
            byteString.EnsureCapacity(capacity);
            return byteString;
        }

        public static ByteString Create(Stream stream)
        {
            ByteString byteString = ThreadSafePool<ByteString>.Get();
            byteString.Clear();
            byteString.WriteFromStream(stream);
            return byteString;
        }

        public static ByteString Create(ReadOnlySpan<byte> bytes)
        {
            ByteString byteString = ThreadSafePool<ByteString>.Get();
            byteString.Clear();
            byteString.WriteBytes(bytes);
            return byteString;
        }

        ~ByteString()
        {
            FreeMemory();
        }

        public void Dispose()
        {
            Clear();
            if (_capacity > MEMORY_THRESHOLD)
            {
                FreeMemory();
                _capacity = 0;
            }
            ThreadSafePool<ByteString>.Release(this);
        }

        public void WriteFromStream(Stream stream)
        {
            EnsureCapacity(_count + (int)stream.Length);
            stream.Read(new Span<byte>(_buffer + _count, (int)stream.Length));
            _count += (int)stream.Length;
        }

        public void WriteBytes(ReadOnlySpan<byte> bytes)
        {
            int size = bytes.Length;
            EnsureCapacity(_count + size);
            fixed (void* ptr = &MemoryMarshal.GetReference(bytes))
            {
                Buffer.MemoryCopy(ptr, _buffer + _count, size, size);
            }
            _count += size;
        }

        public void WriteValue<T>(ref T value) where T : unmanaged
        {
            EnsureCapacity(_count + sizeof(T));
            *(T*)(_buffer + _count) = value;
            _count += sizeof(T);
        }

        public void WriteValue<T>(T value) where T : unmanaged
        {
            EnsureCapacity(_count + sizeof(T));
            *(T*)(_buffer + _count) = value;
            _count += sizeof(T);
        }

        public void WriteValues<T>(ReadOnlySpan<T> values) where T : unmanaged
        {
            ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(values);
            WriteBytes(bytes);
        }

        public void WriteChars(ReadOnlySpan<char> str)
        {
            ReadOnlySpan<byte> bytes = MemoryMarshal.AsBytes(str);
            WriteValue(bytes.Length);
            WriteBytes(bytes);
        }

        public void WriteUTF8String(ReadOnlySpan<char> str)
        {
            int byteCount = Encoding.UTF8.GetByteCount(str);
            WriteValue(byteCount);
            EnsureCapacity(_count + byteCount);
            Encoding.UTF8.GetBytes(str, new Span<byte>(_buffer + _count, byteCount));
            _count += byteCount;
        }

        public void WriteByteString(ByteString byteString)
        {
            if (byteString == null)
            {
                WriteValue(0);
                return;
            }
            WriteValue(byteString._count);
            WriteBytes(byteString.AsSpan());
        }

        public ref T ReadValue<T>(int position) where T : unmanaged
        {
            CheckPosition(position, sizeof(T));
            return ref *(T*)(_buffer + position);
        }

        public Span<T> ReadValues<T>(int position, int length) where T : unmanaged
        {
            CheckPosition(position, length * sizeof(T));
            return new Span<T>(_buffer + position, length);
        }

        /// <summary>
        /// Reads a sequence of characters from the ByteString starting at the specified position.
        /// Returns the number of bytes read.
        /// </summary>
        public int ReadChars(int position, out Span<char> chars)
        {
            CheckPosition(position, sizeof(int));
            int byteCount = ReadValue<int>(position);
            chars = ReadValues<char>(position + sizeof(int), byteCount / sizeof(char));
            return sizeof(int) + byteCount;
        }

        /// <summary>
        /// Reads a UTF-8 encoded string from the ByteString starting at the specified position.
        /// Returns the number of bytes read.
        /// </summary>
        public int ReadUTF8String(int position, out string value)
        {
            CheckPosition(position, sizeof(int));
            int byteCount = ReadValue<int>(position);
            CheckPosition(position + sizeof(int), byteCount);
            value = Encoding.UTF8.GetString(_buffer + position + sizeof(int), byteCount);
            return sizeof(int) + byteCount;
        }


        /// <summary>
        /// Reads a ByteString from the specified position.
        /// Returns the number of bytes read.
        /// </summary>
        public int ReadByteString(int position, out ByteString byteString)
        {
            CheckPosition(position, sizeof(int));
            int byteCount = ReadValue<int>(position);
            CheckPosition(position + sizeof(int), byteCount);
            byteString = ByteString.Create(new Span<byte>(_buffer + position + sizeof(int), byteCount));
            return sizeof(int) + byteCount;
        }

        public BytesReader GetBytesReader(int position = 0)
        {
            return new BytesReader(this, position);
        }

        public void Clear()
        {
            _count = 0;
        }

        public Span<byte> AsSpan()
        {
            return new Span<byte>(_buffer, _count);
        }

        private void EnsureCapacity(int minSize)
        {
            if (_capacity <= minSize)
            {
                int newCapacity = _capacity == 0 ? DEFAULT_CAPACITY : _capacity * 2;
                if ((uint)newCapacity > MAX_SIZE) newCapacity = MAX_SIZE;
                if (newCapacity < minSize) newCapacity = minSize;

                byte* newBuffer = Allocate(newCapacity);
                Buffer.MemoryCopy(_buffer, newBuffer, newCapacity, _count);
                FreeMemory();
                _capacity = newCapacity;
                _buffer = newBuffer;
            }
        }

        private static byte* Allocate(int length)
        {
            if (length < 1)
                return null;

            return (byte*)Marshal.AllocHGlobal(length);
        }

        private void FreeMemory()
        {
            if (_buffer != null)
            {
                Marshal.FreeHGlobal((IntPtr)_buffer);
                _buffer = null;
            }
        }

        private void CheckPosition(int position, int size)
        {
            if (position < 0 || position + size > _count)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }
        }

        public struct BytesReader
        {
            private readonly ByteString _byteString;
            public int Position { get; private set; }


            public BytesReader(ByteString byteString, int position)
            {
                if (position < 0 || position > byteString._count)
                {
                    throw new ArgumentOutOfRangeException(nameof(position));
                }
                _byteString = byteString;
                Position = position;
            }

            public ref T ReadValue<T>() where T : unmanaged
            {
                ref T value = ref _byteString.ReadValue<T>(Position);
                Position += sizeof(T);
                return ref value;
            }

            public Span<T> ReadValues<T>(int length) where T : unmanaged
            {
                Span<T> values = _byteString.ReadValues<T>(Position, length);
                Position += length * sizeof(T);
                return values;
            }

            public Span<char> ReadChars()
            {
                Position += _byteString.ReadChars(Position, out Span<char> chars);
                return chars;
            }

            public string ReadUTF8String()
            {
                Position += _byteString.ReadUTF8String(Position, out string str);
                return str;
            }

            public string ReadString()
            {
                Position += _byteString.ReadChars(Position, out Span<char> chars);
                return chars.ToString();
            }

            public ByteString ReadByteString()
            {
                Position += _byteString.ReadByteString(Position, out ByteString byteString);
                return byteString;
            }
        }
    }
}