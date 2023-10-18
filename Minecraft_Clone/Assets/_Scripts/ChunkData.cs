using UnityEngine;
using UnityEngine.UIElements;
using static GameSettings;

public enum ChunkState : byte
{
    Generated,
    PreparingMesh,
    MeshPrepared,
    Rendering,
}

public class ChunkData
{
    public readonly BlockType[] blocks = new BlockType[TotalBlockInChunk];
    public readonly Vector3Int worldPosition;
    public readonly Vector3Int chunkCoord;

    public readonly World world;

    public ChunkState state;

    public ChunkData(World world ,Vector3Int chunkCoord)
    {
        this.world = world;
        this.chunkCoord = chunkCoord;
        worldPosition = chunkCoord * ChunkSize;
    }

    public void SetBlock(Vector3Int localPos, BlockType type)
    {
        int index = localPos.x + ChunkSize.x * (localPos.y + ChunkSize.z * localPos.z);
        blocks[index] = type;
    }

    public BlockType GetBlock(Vector3Int localPos)
    {
        int index = localPos.x + ChunkSize.x * (localPos.y + ChunkSize.z * localPos.z);
        return blocks[index];
    }
}

