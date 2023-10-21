using System.Collections.Concurrent;
using UnityEngine;

public static class ConcurrentPool
{
    private static ConcurrentQueue<MeshData> _meshDataPool = new();

    public static MeshData GetMeshData()
    {
        if(!_meshDataPool.TryDequeue(out var meshData))
        {
            meshData = new MeshData();
        }
        meshData.Clear();
        return meshData;
    }

    public static void Release(MeshData meshData)
    {
        _meshDataPool.Enqueue(meshData);
    }

    private static ConcurrentQueue<ChunkData> _chunkDatasPool = new();

    public static ChunkData GetChunkData(Vector3Int chunkCoord)
    {
        if (!_chunkDatasPool.TryDequeue(out var chunkData) || chunkData.state != ChunkState.InPool)
        {
            chunkData = new ChunkData();
            chunkData.SetChunkCoord(chunkCoord);
            return chunkData;
        }
        chunkData.SetChunkCoord(chunkCoord);
        chunkData.modifiedByPlayer = false;
        chunkData.isDirty = false;
        chunkData.structures.Clear();
        chunkData.modifierQueue.Clear();
        return chunkData;
    }

    public static void Release(ChunkData chunkData)
    {
        chunkData.state = ChunkState.InPool;
        _chunkDatasPool.Enqueue(chunkData);
    }
}