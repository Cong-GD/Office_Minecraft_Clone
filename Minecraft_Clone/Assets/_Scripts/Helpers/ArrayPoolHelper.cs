using System;
using System.Buffers;

public static class ArrayPoolHelper
{
    public static SharedObject<T> Rent<T>(int minimumLength)
    {
        return new SharedObject<T>(minimumLength);
    }

    public struct SharedObject<T> : IDisposable
    {
        private readonly T[] _value;

        public SharedObject(int minimumLength)
        {
            _value = ArrayPool<T>.Shared.Rent(minimumLength);
        }

        public readonly T[] Value
        {
            get
            {
                return _value;
            }
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(Value);
        }
    }
}