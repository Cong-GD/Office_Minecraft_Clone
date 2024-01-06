using System;
using System.Runtime.CompilerServices;

[Serializable]
public struct StatPoint : IEquatable<StatPoint>
{
    public enum Amount
    {
        Empty, Half, Full
    }

    public enum State
    {
        Health, PoisonedHealth, AbsorbingHealth, Oxygen, Armor, Food, Saturation,
    }

    public Amount amount;

    public State state;

    public static bool operator ==(StatPoint p1, StatPoint p2)
    {
        return p1.Equals(p2);
    }

    public static bool operator !=(StatPoint p1, StatPoint p2)
    {
        return !p1.Equals(p2);
    }

    public readonly override bool Equals(object obj) => obj is StatPoint healthPoint && Equals(healthPoint);

    public readonly bool Equals(StatPoint obj) => amount == obj.amount && state == obj.state;

    public readonly override int GetHashCode() => (int)state * 10 + (int)amount;
}