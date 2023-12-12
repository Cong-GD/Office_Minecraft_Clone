using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static WorldSettings;

public static class Chunk
{
    public static MeshData GetMeshData(ChunkData chunkData)
    {
        MeshData meshData = ThreadSafePool<MeshData>.Get();
        meshData.Clear();
        meshData.position = chunkData.chunkCoord;
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < CHUNK_DEPTH; y++)
            {
                for(int z = 0; z < CHUNK_WIDTH; z++)
                {
                    chunkData.GetBlockUncheck(x, y, z)
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
        return (uint)localPos.x < CHUNK_WIDTH
            && (uint)localPos.y < CHUNK_DEPTH
            && (uint)localPos.z < CHUNK_WIDTH;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositionInChunk(int localX, int localY, int localZ)
    {
        return (uint)localX < CHUNK_WIDTH
            && (uint)localY < CHUNK_DEPTH
            && (uint)localZ < CHUNK_WIDTH;
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
        return (uint)y < MAP_HEIGHT_IN_CHUNK;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidWorldY(int y)
    {
        return (uint)y < MAP_HEIGHT_IN_BLOCK;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidLocalY(int y)
    {
        return (uint)y < CHUNK_DEPTH;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositionInRange(Vector3Int center, Vector3Int position, int range)
    {
        return Mathf.Abs(center.x - position.x) <= range 
            && Mathf.Abs(center.z - position.z) <= range;
    }

    public static BlockType GetBlock(ChunkData chunkData, int localX, int localY, int localZ)
    {
        if (IsPositionInChunk(localX, localY, localZ))
            return chunkData.GetBlockUncheck(localX, localY, localZ);

        return GetBlock(chunkData.worldPosition.x + localX,
                        chunkData.worldPosition.y + localY,
                        chunkData.worldPosition.z + localZ);
    }

    public static BlockType GetBlock(Vector3 worldPos)
    {
        return GetBlock(
            Mathf.FloorToInt(worldPos.x), 
            Mathf.FloorToInt(worldPos.y), 
            Mathf.FloorToInt(worldPos.z)
            );
    }

    public static BlockType GetBlock(int worldX, int worldY, int worldZ)
    {
        var chunkCoord = GetChunkCoord(worldX, worldY, worldZ);

        if (World.Instance.TryGetChunkData(chunkCoord, out var chunkData))
        {
            return chunkData.GetBlockUncheck(
                         worldX - chunkData.worldPosition.x,
                         worldY - chunkData.worldPosition.y,
                         worldZ - chunkData.worldPosition.z);
        }

        return BlockType.Air;
    }

    public static Direction GetDirection(Vector3 worldPosition)
    {
        return GetDirection(
            Mathf.FloorToInt(worldPosition.x), 
            Mathf.FloorToInt(worldPosition.y), 
            Mathf.FloorToInt(worldPosition.z)
            );
    }

    public static Direction GetDirection(int worldX, int worldY, int worldZ)
    {
        var chunkCoord = GetChunkCoord(worldX, worldY, worldZ);

        if (World.Instance.TryGetChunkData(chunkCoord, out var chunkData))
        {
            return chunkData.GetDirectionUncheck(
                         worldX - chunkData.worldPosition.x,
                         worldY - chunkData.worldPosition.y,
                         worldZ - chunkData.worldPosition.z);
        }

        return Direction.Forward;
    }

    public static bool CheckWater(Vector3 worldPosition)
    {
        return GetBlock(
               Mathf.FloorToInt(worldPosition.x), 
               Mathf.FloorToInt(worldPosition.y), 
               Mathf.FloorToInt(worldPosition.z))
               .Data()
               .BlockType == BlockType.Water;
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
            chunkData.SetBlockUncheck(
                         worldX - chunkData.worldPosition.x,
                         worldY - chunkData.worldPosition.y,
                         worldZ - chunkData.worldPosition.z, 
                         blockType, direction);
        }
    }

    public static IEnumerable<Vector3Int> GetCoordsInRange(Vector3Int center, int range)
    {
        int xStart = center.x - range;
        const int yStart = 0;
        int zStart = center.z - range;

        int xEnd = center.x + range;
        const int yEnd = MAP_HEIGHT_IN_CHUNK;
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

    public static IEnumerable<Vector3Int> GetAdjacentChunkCoords(Vector3Int worldPosition)
    {
        Vector3Int center = GetChunkCoord(worldPosition);
        yield return center;
        foreach (var direction in VectorExtensions.SixDirectionsVector3Int)
        {
            var newCoord = GetChunkCoord(worldPosition + direction);
            if (newCoord != center)
            {
                yield return newCoord;
            }
        }
    }
}