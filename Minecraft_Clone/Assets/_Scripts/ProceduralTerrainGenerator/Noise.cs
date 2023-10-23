using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public static class Noise
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Get2DPerlin(Vector2Int position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / GameSettings.CHUNK_WIDTH * scale + offset, (position.y + 0.1f) / GameSettings.CHUNK_WIDTH * scale + offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Get3DPerlin(Vector3Int pos, float offset, float scale, float threashold)
    {
        float x = (pos.x + offset + 0.1f) * scale;
        float y = (pos.y + offset + 0.1f) * scale;
        float z = (pos.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        return (AB + BC + AC + BA + CB + CA) / 6 > threashold;
    }

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
    public static int RemapValue01ToInt(float value, float outputMin, float outputMax)
    {
        return (int)RemapValue01(value, outputMin, outputMax);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Redistribution(float noise, NoiseSettings settings)
    {
        return Mathf.Pow(noise * settings.redistributionModifier, settings.exponent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float OctavePerlin(float x, float z, NoiseSettings settings)
    {
        x = x * settings.noiseZoom + 0.1f + settings.offest.x;
        z = z * settings.noiseZoom + 0.1f + settings.offest.y;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
        for (int i = 0; i < settings.octaves; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;

            amplitudeSum += amplitude;

            amplitude *= settings.persistance;
            frequency *= 2;
        }
        return total / amplitudeSum;
    }
}
