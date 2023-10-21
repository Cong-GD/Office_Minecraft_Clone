using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static GameSettings;

public enum ChunkState : byte
{
    Creating,
    Generated,
    PreparingMesh,
    MeshPrepared,
    Rendering,
    InPool
}

public class ChunkData
{
    public readonly BlockType[] blocks = new BlockType[TotalBlockInChunk];
    public Vector3Int worldPosition;
    public Vector3Int chunkCoord;

    public ChunkState state;

    public bool isDirty;
    public bool modifiedByPlayer;

    public readonly List<(Vector3Int, Structure)> structures = new();

    public readonly Queue<ModifierUnit> modifierQueue = new();

    public void SetChunkCoord(Vector3Int chunkCoord)
    {
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

    /// <summary>
    ///  More optimize way to set block, use it carefully
    /// </summary>
    internal void SetBlock(ref Vector3Int localPos, BlockType type)
        => blocks[localPos.x + ChunkSize.x * (localPos.y + ChunkSize.z * localPos.z)] = type;
    /// <summary>
    ///  More optimize way to get block, use it carefully
    /// </summary>
    internal BlockType GetBlock(ref Vector3Int localPos)
        => blocks[localPos.x + ChunkSize.x * (localPos.y + ChunkSize.z * localPos.z)];

    public bool HasStructure() => structures.Count > 0;

    public bool HasModifier() => modifierQueue.Any();
}