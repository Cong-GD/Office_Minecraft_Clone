using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static GameSettings;

public static class Chunk
{
    public static MeshData GetMeshData(ChunkData chunkData)
    {
        MeshData meshData = new MeshData();

        var pos = Vector3Int.zero;
        for (pos.x = 0; pos.x < ChunkSize.x; pos.x++)
        {
            for (pos.y = 0; pos.y < ChunkSize.y; pos.y++)
            {
                for(pos.z = 0; pos.z < ChunkSize.z; pos.z++)
                {
                    BlockHelper.AddBlockMeshData(chunkData, meshData, pos);
                }
            }
        }

        return meshData;
    }

    public static bool IsPositionInChunk(Vector3Int localPos)
    {
        return localPos.x > -1 && localPos.x < ChunkSize.x
            && localPos.y > -1 && localPos.y < ChunkSize.y
            && localPos.z > -1 && localPos.z < ChunkSize.z;
    }

    public static Vector3Int GetChunkCoord(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / ChunkSize.x);
        int y = Mathf.FloorToInt(worldPosition.y / ChunkSize.y);
        int z = Mathf.FloorToInt(worldPosition.z / ChunkSize.z);
        return new Vector3Int(x, y, z);
    }

    public static bool IsValidChunkCoordY(int y)
    {
        return y > -1 && y < ChunkSize.y;
    }

    public static IEnumerable<Vector3Int> GetCoordsInRange(Vector3Int center, int range)
    {
        int xStart = center.x - range;
        int yStart = 0;
        int zStart = center.z - range;
        int xEnd = center.x + range;
        int yEnd = MapHeightInChunk;
        int zEnd = center.z + range;
        Vector3Int pos = new Vector3Int(xStart, yStart, zStart);
        for (pos.x = xStart;pos.x <= xEnd; pos.x++)
        {
            for (pos.y = yStart;pos.y < yEnd; pos.y++)
            {
                for (pos.z = zStart; pos.z <= zEnd; pos.z++)
                {
                    yield return pos;
                }
            }
        }
    }

    public static bool IsPositionInRange(Vector3Int center, Vector3Int position, int range)
    {
        return Mathf.Abs(center.x - position.x) <= range && Mathf.Abs(center.z - position.z) <= range;
    }

    public static HashSet<Vector3Int> GetAdjacentChunkCoords(Vector3Int worldPosition)
    {
        HashSet<Vector3Int> set = new();
        foreach (var direction in DirectionExtensions.GetDirections())
        {
            set.Add(GetChunkCoord(worldPosition + direction.GetVector()));
        }
        return set;
    }

}