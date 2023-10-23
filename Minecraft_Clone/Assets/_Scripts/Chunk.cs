using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;
using static GameSettings;

public static class Chunk
{
    public static MeshData GetMeshData(ChunkData chunkData)
    {
        MeshData meshData = ThreadSafePool<MeshData>.Get(); //ConcurrentPool.GetMeshData();
        meshData.Clear();

        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < CHUNK_DEPTH; y++)
            {
                for(int z = 0; z < CHUNK_WIDTH; z++)
                {
                    //var blockData = BlockDataManager.GetData(chunkData.GetBlock(pos));
                    //blockData.MeshGenerator.GetMeshData(meshData, chunkData, pos);
                    BlockHelper.AddBlockMeshData(chunkData, meshData, new (x, y, z));
                }
            }
        }

        return meshData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositionInChunk(Vector3Int localPos)
    {
        return localPos.x > -1 && localPos.x < CHUNK_WIDTH
            && localPos.y > -1 && localPos.y < CHUNK_DEPTH
            && localPos.z > -1 && localPos.z < CHUNK_WIDTH;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int GetChunkCoord(Vector3Int worldPosition)
    {
        int x = Mathf.FloorToInt((float)worldPosition.x / CHUNK_WIDTH);
        int y = Mathf.FloorToInt((float)worldPosition.y / CHUNK_DEPTH);
        int z = Mathf.FloorToInt((float)worldPosition.z / CHUNK_WIDTH);
        return new Vector3Int(x, y, z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidChunkCoordY(int y)
    {
        return y > -1 && y < MAP_HEIGHT_IN_CHUNK;
    }

    public static IEnumerable<Vector3Int> GetCoordsInRange(Vector3Int center, int range)
    {
        int xStart = center.x - range;
        int yStart = 0;
        int zStart = center.z - range;
        int xEnd = center.x + range;
        int yEnd = MAP_HEIGHT_IN_CHUNK;
        int zEnd = center.z + range;
        Vector3Int pos = new Vector3Int();
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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