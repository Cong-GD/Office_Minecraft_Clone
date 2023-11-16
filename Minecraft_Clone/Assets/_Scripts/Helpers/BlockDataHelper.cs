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
        var blockResources = Resources.LoadAll<BlockData_SO>("BlockDatas");
        var blockTypeCount = Enum.GetNames(typeof(BlockType)).Length;

        _blockDataMap = new BlockData_SO[blockTypeCount];
        foreach (var item in blockResources)
        {
            _blockDataMap[(byte)item.BlockType] = item;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BlockData_SO Data(this BlockType type)
    {
        return _blockDataMap[(byte)type];
    }
}