using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Vector2Int;

public static class Biome
{
    public static int WaterLevel = 140;

    public static readonly int BiomeCheckRangeInBlock = 50;


    public static Vector2Int[] neighbours8Directions =
    {
        up, 
        one,
        right,
        right + down,
        down, left + down,
        left, 
        left + up
    };

    public static HashSet<Vector3Int> CalculateBiomeCenter(Vector3 playerPosition, int drawRange)
    {
        
        int biomeLength = drawRange * WorldSettings.CHUNK_WIDTH;
        Vector3Int origin = new Vector3Int(
            Mathf.RoundToInt(playerPosition.x / biomeLength) * biomeLength,
            0,
            Mathf.RoundToInt(playerPosition.z / biomeLength) * biomeLength);
        HashSet<Vector3Int> biomeCenters = new() { origin };

        foreach (Vector2Int offsetXZ in neighbours8Directions)
        {
            Vector3Int newBiomPoint_1 = new Vector3Int(origin.x + offsetXZ.x * biomeLength, 0, origin.z + offsetXZ.y * biomeLength);
            Vector3Int newBiomPoint_2 = new Vector3Int(origin.x + offsetXZ.x * biomeLength, 0, origin.z + offsetXZ.y * 2 * biomeLength);
            Vector3Int newBiomPoint_3 = new Vector3Int(origin.x + offsetXZ.x * 2 * biomeLength, 0, origin.z + offsetXZ.y * biomeLength);
            Vector3Int newBiomPoint_4 = new Vector3Int(origin.x + offsetXZ.x * 2 * biomeLength, 0, origin.z + offsetXZ.y * 2 * biomeLength);
            biomeCenters.Add(newBiomPoint_1);
            biomeCenters.Add(newBiomPoint_2);
            biomeCenters.Add(newBiomPoint_3);
            biomeCenters.Add(newBiomPoint_4);
        }
        return biomeCenters;
    }
}
