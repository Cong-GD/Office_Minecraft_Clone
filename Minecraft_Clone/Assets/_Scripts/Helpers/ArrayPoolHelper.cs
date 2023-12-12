using System;
using System.Buffers;

public static class ArrayPoolHelper
{
    public static SharedObject<T> Rent<T>(int minimumLength, bool clearOnReturn = false)
    {
        return new SharedObject<T>(minimumLength, clearOnReturn);
    }

    public readonly struct SharedObject<T> : IDisposable
    {
        private readonly T[] _value;
        private readonly bool _clearOnreturn;

        public SharedObject(int minimumLength, bool clearOnreturn)
        {
            _value = ArrayPool<T>.Shared.Rent(minimumLength);
            this._clearOnreturn = clearOnreturn;
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
            ArrayPool<T>.Shared.Return(Value, _clearOnreturn);
        }
    }
}