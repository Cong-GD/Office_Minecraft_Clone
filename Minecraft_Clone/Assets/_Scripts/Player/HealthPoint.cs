
using System;
using System.Runtime.CompilerServices;

[Serializable]
public struct HealthPoint
{
    public enum Amount
    {
        Empty, Half, Full
    }

    public enum State
    {
        Normal, Poisoned, Absorbing
    }

    public Amount amount;

    public State state;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(HealthPoint p1, HealthPoint p2)
    {
        return p1.Equals(p2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(HealthPoint p1, HealthPoint p2)
    {
        return !p1.Equals(p2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override bool Equals(object obj)
    {
        return obj is HealthPoint healthPoint && Equals(healthPoint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(HealthPoint obj) => amount == obj.amount && state == obj.state;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly override int GetHashCode()
    {
        return (int)amount * 10 + (int)state;
    }
}

