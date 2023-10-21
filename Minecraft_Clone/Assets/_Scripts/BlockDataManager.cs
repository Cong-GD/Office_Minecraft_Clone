﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockDataManager : MonoBehaviour
{

    private static Dictionary<BlockType, BlockData> _blockDataMap;

    /// <summary>
    /// Don't use this method on awake
    /// </summary>
    public static BlockData GetData(BlockType type)
    {
        if(_blockDataMap.TryGetValue(type, out var data))
        {
            return data;
        }
        throw new System.Exception($"Block of type {type} does not exits");
    }

    private void Awake()
    {
        _blockDataMap = Resources.LoadAll<BlockData>("BlockDatas").ToDictionary(data => data.blockType, data => data);
    }


}