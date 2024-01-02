using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class BlockDataHelper
{

    private static BlockData_SO[] _blockDataMap;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var blockResources = Resources.LoadAll<BlockData_SO>("Items");
        var blockTypeCount = Enum.GetNames(typeof(BlockType)).Length;

        _blockDataMap = new BlockData_SO[blockTypeCount];
        foreach (var block in blockResources)
        {
            if (_blockDataMap[(byte)block.BlockType] != null)
            {
                Debug.LogError($"Block type: {block.BlockType} is duplicated");
            }

            _blockDataMap[(byte)block.BlockType] = block;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BlockData_SO Data(this BlockType blockType)
    {
        return _blockDataMap[(byte)blockType];
    }
}