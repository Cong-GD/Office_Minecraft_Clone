using Minecraft.ProceduralTerrain.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static WorldSettings;

public enum ChunkState : byte
{
    InPool,
    Creating,
    Generated,
    PreparingMesh,
    MeshPrepared,
    Rendering,
}

[Serializable]
public class ChunkData
{
    public readonly BlockType[] blocks = new BlockType[TOTAL_BLOCK_IN_CHUNK];

    public Vector3Int worldPosition;
    public Vector3Int chunkCoord;

    public ChunkState state = ChunkState.InPool;

    public bool isDirty;
    public bool modifiedByPlayer;

    public List<(Vector3Int, IStructure)> structures = new();
    public Queue<ModifierUnit> modifierQueue = new();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndex(int x, int y, int z)
        => (z * CHUNK_WIDTH * CHUNK_DEPTH) + (y * CHUNK_WIDTH) + x;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetChunkCoord(Vector3Int chunkCoord)
    {
        this.chunkCoord = chunkCoord;
        worldPosition = chunkCoord * ChunkSizeVector;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlock(Vector3Int localPos, BlockType block)
    {
        blocks[GetIndex(localPos.x, localPos.y, localPos.z)] = block;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetBlock(Vector3Int localPos)
    {
        return blocks[GetIndex(localPos.x, localPos.y, localPos.z)];
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlock(int x, int y, int z, BlockType block)
    {
        blocks[GetIndex(x, y, z)] = block;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetBlock(int x, int y, int z)
    {
        return blocks[GetIndex(x, y, z)];
    }



    public bool HasStructure() => structures.Count > 0;



    public bool HasModifier() => modifierQueue.Any();
}