using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;
using static WorldSettings;

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
                    chunkData.GetBlock(x, y, z)
                            .Data()
                            .MeshGenerator
                            .GetMeshData(chunkData, meshData, x, y, z);
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
    public static bool IsPositionInChunk(int localX, int localY, int localZ)
    {
        return localX > -1 && localX < CHUNK_WIDTH
            && localY > -1 && localY < CHUNK_DEPTH
            && localZ > -1 && localZ < CHUNK_WIDTH;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int GetChunkCoord(Vector3 worldPos)
    {
        return GetChunkCoord(worldPos.x, worldPos.y, worldPos.z);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int GetChunkCoord(float worldX, float worldY, float worldZ)
    {
        return new Vector3Int
        {
            x = Mathf.FloorToInt(worldX / CHUNK_WIDTH),
            y = Mathf.FloorToInt(worldY / CHUNK_DEPTH),
            z = Mathf.FloorToInt(worldZ / CHUNK_WIDTH)
        };
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidChunkCoordY(int y)
    {
        return y > -1 && y < MAP_HEIGHT_IN_CHUNK;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidWorldY(int y)
    {
        return y > -1 && y < MAP_HEIGHT_IN_BLOCK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidLocalY(int y)
    {
        return y > -1 && y < CHUNK_DEPTH;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetBlockIndex(int localX, int localY, int localZ)
        => (localZ * CHUNK_WIDTH * CHUNK_DEPTH) + (localY * CHUNK_WIDTH) + localX;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositionInRange(Vector3Int center, Vector3Int position, int range)
    {
        return Mathf.Abs(center.x - position.x) <= range && Mathf.Abs(center.z - position.z) <= range;
    }

    public static BlockType GetBlock(ChunkData chunkData, int localX, int localY, int localZ)
    {
        if (IsPositionInChunk(localX, localY, localZ))
            return chunkData.blocks[GetBlockIndex(localX, localY, localZ)];

        return GetBlock(chunkData.worldPosition.x + localX,
                        chunkData.worldPosition.y + localY,
                        chunkData.worldPosition.z + localZ);
    }

    public static BlockType GetBlock(Vector3Int worldPos)
    {
        return GetBlock(worldPos.x, worldPos.y, worldPos.z);
    }

    public static BlockType GetBlock(int worldX, int worldY, int worldZ)
    {
        var chunkCoord = GetChunkCoord(worldX, worldY, worldZ);

        if (World.Instance.TryGetChunkData(chunkCoord, out var chunkData))
        {
            return chunkData.GetBlock(
                         worldX - chunkData.worldPosition.x,
                         worldY - chunkData.worldPosition.y,
                         worldZ - chunkData.worldPosition.z);
        }

        return BlockType.Air;
    }

    public static Direction GetDirection(Vector3Int worldPosition)
    {
        return GetDirection(worldPosition.x, worldPosition.y, worldPosition.z);
    }

    public static Direction GetDirection(int worldX, int worldY, int worldZ)
    {
        var chunkCoord = GetChunkCoord(worldX, worldY, worldZ);

        if (World.Instance.TryGetChunkData(chunkCoord, out var chunkData))
        {
            return chunkData.GetDirection(
                         worldX - chunkData.worldPosition.x,
                         worldY - chunkData.worldPosition.y,
                         worldZ - chunkData.worldPosition.z);
        }

        return Direction.Forward;
    }

    public static void SetBlock(Vector3Int worldPos, BlockType blockType, Direction direction = Direction.Forward)
    {
        SetBlock(worldPos.x, worldPos.y, worldPos.z, blockType, direction);
    }

    public static void SetBlock(int worldX, int worldY, int worldZ, BlockType blockType, Direction direction = Direction.Forward)
    {
        var chunkCoord = GetChunkCoord(worldX, worldY, worldZ);

        if (World.Instance.TryGetChunkData(chunkCoord, out var chunkData))
        {
            chunkData.SetBlock(
                         worldX - chunkData.worldPosition.x,
                         worldY - chunkData.worldPosition.y,
                         worldZ - chunkData.worldPosition.z, 
                         blockType, direction);
        }
    }

    public static IEnumerable<Vector3Int> GetCoordsInRange(Vector3Int center, int range)
    {
        int xStart = center.x - range;
        int yStart = 0;
        int zStart = center.z - range;
        int xEnd = center.x + range;
        int yEnd = MAP_HEIGHT_IN_CHUNK;
        int zEnd = center.z + range;
        for (int x = xStart;x <= xEnd; x++)
        {
            for (int y = yStart;y < yEnd; y++)
            {
                for (int z = zStart; z <= zEnd; z++)
                {
                    yield return new Vector3Int(x, y, z);
                }
            }
        }
    }

    public static HashSet<Vector3Int> GetAdjacentChunkCoords(Vector3Int worldPosition)
    {
        HashSet<Vector3Int> set = new();
        foreach (var direction in VectorExtensions.SixDirectionsVector3Int)
        {
            set.Add(GetChunkCoord(worldPosition + direction));
        }
        return set;
    }
}