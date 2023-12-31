﻿#define UNSAFE

using CongTDev.Collection;
using Minecraft;
using Minecraft.ProceduralTerrain.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

#if UNSAFE
public unsafe class ChunkData
#else
public class ChunkData
#endif
{
    /// <summary>
    /// This is an read only field, please don't modify this value,
    /// because of performance issue i can't make this as get only property
    /// </summary>
    public Vector3Int worldPosition;

    public Vector3Int chunkCoord;

    public ChunkState state = ChunkState.InPool;

    public bool isDirty;

    public readonly List<ValueTuple<Vector3Int, IStructure>> structures = new();
    public readonly Queue<ModifierUnit> modifierQueue = new();

#if UNSAFE
    private readonly BlockType* _blocks;
    private readonly Direction* _blockDirections;
#else
    private readonly BlockType[] _blocks = new BlockType[TOTAL_BLOCK_IN_CHUNK];
    private readonly Direction[] _blockDirections = new Direction[TOTAL_BLOCK_IN_CHUNK];
#endif

#if UNSAFE
    public ChunkData()
    {
        _blocks = (BlockType*)Marshal.AllocHGlobal(TOTAL_BLOCK_IN_CHUNK * (sizeof(BlockType) + sizeof(Direction)));
        _blockDirections = (Direction*)(_blocks + TOTAL_BLOCK_IN_CHUNK);
    }
    ~ChunkData()
    {
        Marshal.FreeHGlobal((IntPtr)_blocks);
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasStructure() => structures.Count > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasModifier() => modifierQueue.Count > 0;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndex(int x, int y, int z)
        => (z * CHUNK_WIDTH * CHUNK_DEPTH) + (y * CHUNK_WIDTH) + x;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIndexOptimized(int x, int y, int z)
        => CHUNK_WIDTH * (z * CHUNK_DEPTH + y) + x;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetChunkCoord(Vector3Int chunkCoord)
    {
        this.chunkCoord = chunkCoord;
        worldPosition = chunkCoord * ChunkSizeVector;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlock(int x, int y, int z, BlockType block, Direction direction = Direction.Forward)
    {
        int index = GetIndexOptimized(x, y, z);
#if UNSAFE
        if ((uint)index >= TOTAL_BLOCK_IN_CHUNK)
            throw new IndexOutOfRangeException($"Set block: ({x} , {y}, {z})");
#endif
        _blocks[index] = block;
        _blockDirections[index] = direction;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBlockUncheck(int x, int y, int z, BlockType block, Direction direction = Direction.Forward)
    {
        int index = GetIndexOptimized(x, y, z);
        _blocks[index] = block;
        _blockDirections[index] = direction;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetBlock(Vector3Int localPosition)
    {
        return GetBlock(localPosition.x, localPosition.y, localPosition.z);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetBlock(int x, int y, int z)
    {
        int index = GetIndexOptimized(x, y, z);
#if UNSAFE
        if ((uint)index >= TOTAL_BLOCK_IN_CHUNK)
            throw new IndexOutOfRangeException($"Get block ({x} , {y}, {z})");
#endif
        return _blocks[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockType GetBlockUncheck(int x, int y, int z)
    {
        return _blocks[GetIndexOptimized(x, y, z)];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Direction GetDirection(int x, int y, int z)
    {
        int index = GetIndexOptimized(x, y, z);
#if UNSAFE
        if ((uint)index >= TOTAL_BLOCK_IN_CHUNK)
            throw new IndexOutOfRangeException($"Get direction ({x} , {y}, {z})");
#endif
        return _blockDirections[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Direction GetDirectionUncheck(int x, int y, int z)
    {
        return _blockDirections[GetIndexOptimized(x, y, z)];
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlockAndDirection(int x, int y, int z, out BlockType block, out Direction direction)
    {
        int index = GetIndexOptimized(x, y, z);
#if UNSAFE
        if ((uint)index >= TOTAL_BLOCK_IN_CHUNK)
            throw new IndexOutOfRangeException($"Get block and direction ({x} , {y}, {z})");
#endif
        block = _blocks[index];
        direction = _blockDirections[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetBlockAndDirectionUncheck(int x, int y, int z, out BlockType block, out Direction direction)
    {
        int index = GetIndexOptimized(x, y, z);
        block = _blocks[index];
        direction = _blockDirections[index];
    }
}