using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class BlockDataHelper
{
    private static Dictionary<BlockType, BlockData_SO> _blockDataMap;

    /// <summary>
    /// This method must to call somewhere before try to get block data
    /// I don't use static constructor because of performance problem
    /// </summary>
    public static void Init()
    {
        _blockDataMap = Resources.LoadAll<BlockData_SO>("BlockDatas").ToDictionary(data => data.BlockType, data => data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BlockData_SO Data(this BlockType type)
    {
        if (_blockDataMap.TryGetValue(type, out var data))
        {
            return data;
        }
        throw new System.Exception($"Block of type {type} does not exits");
    }
}