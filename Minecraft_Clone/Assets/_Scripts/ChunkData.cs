using Minecraft.ProceduralTerrain.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
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
    public readonly Direction[] blockDirections = new Direction[TOTAL_BLOCK_IN_CHUNK];
    public Vector3Int worldPosition;
    public Vector3Int chunkCoord;

    public ChunkState state = ChunkState.InPool;

    public bool isDirty;
    public bool modifiedByPlayer;

    public List<(Vector3Int, IStructure)> structures = new();
    public Queue<ModifierUnit> modifierQueue = new();

    public Dictionary<Vector3Int, IBlockState> blockStates = new();

    public bool HasStructure() => structures.Count > 0;

    public bool HasModifier() => modifierQueue.Any();


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
    public void SetBlock(int x, int y, int z, BlockType block, Direction direction = Direction.Forward)
    {
        int index = GetIndex(x, y, z);
        blocks[index] = block;
        blockDirections[index] = direction;
    }

    public BlockType GetBlock(Vector3Int localPosition)
    {
        return GetBlock(localPosition.x, localPosition.y, localPosition.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetBlock(int x, int y, int z)
    {
        return blocks[GetIndex(x, y, z)];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Direction GetDirection(int x, int y, int z)
    {
        return blockDirections[GetIndex(x, y, z)];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlockAndDirection(int x, int y, int z ,out BlockType block,out Direction direction)
    {
        int index = GetIndex(x, y, z);
        block = blocks[index];
        direction = blockDirections[index];
    }

    // Because this array only use for one purpuse and on main theard, so it's safe for cache like this
    private static readonly ArrayBuffer<Vector3Int> _cachedRemoveArray = new();
    public void ValidateBlockState()
    {
        foreach (var blockState in blockStates)
        {
            if(!blockState.Value.Validate())
            {
                _cachedRemoveArray.Add(blockState.Key);
            }
        }

        foreach (var pos in _cachedRemoveArray)
        {
            blockStates.Remove(pos);
        }
        _cachedRemoveArray.Clear();
    }
}