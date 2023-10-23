using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BlockDataManager : MonoBehaviour
{

    private static Dictionary<BlockType, BlockData_SO> _blockDataMap;

    /// <summary>
    /// Don't use this method on awake
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BlockData_SO GetData(BlockType type)
    {
        if(_blockDataMap.TryGetValue(type, out var data))
        {
            return data;
        }
        throw new System.Exception($"Block of type {type} does not exits");
    }

    private void Awake()
    {
        _blockDataMap = Resources.LoadAll<BlockData_SO>("BlockDatas").ToDictionary(data => data.blockType, data => data);
    }


}