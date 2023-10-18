﻿using UnityEngine;

public static class Noise
{
    public static float Get2DPerlin(Vector2Int position, float offset, float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / GameSettings.ChunkSize.x * scale + offset, (position.y + 0.1f) / GameSettings.ChunkSize.z * scale + offset);
    }

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
}