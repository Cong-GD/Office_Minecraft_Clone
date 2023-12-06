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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Max(ReadOnlySpan<int> span)
    {
        if (span.Length == 0)
            throw new ArgumentException("Span cannot be empty.");

        int max = span[0];
        int length = span.Length;
        for (int i = 1; i < length; i++)
        {
            if (span[i] > max)
                max = span[i];
        }
        return max;
    }
}
