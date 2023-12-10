using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class MyMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RemapValue(float value, float initialMin, float initialMax, float outputMin, float outputMax)
    {
        return outputMin + (value - initialMin) * (outputMax - outputMin) / (initialMax - initialMin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RemapValue01(float value, float outputMin, float outputMax)
    {
        return outputMin + value * (outputMax - outputMin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Decimal(float value, int numberBehindDot)
    {
        if (numberBehindDot <= 0)
            return (int)value;

        float dec = Mathf.Pow(10, numberBehindDot);
        return (int)(dec * value) / dec;
    }
}
