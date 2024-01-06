using Minecraft;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceBlock_SO : BlockData_SO, IInteractableBlock
{
    public void Interact(Vector3Int worldPosition)
    {
        if (!World.Instance.TryGetChunkData(Chunk.GetChunkCoord(worldPosition), out var chunkData))
        {
            Debug.LogWarning($"Invalid furnace position {worldPosition}");
            return;
        }

        if (chunkData.GetBlock(worldPosition - chunkData.worldPosition) != BlockType.Furnace)
        {
            Debug.LogWarning($"Can't found furnace at {worldPosition}");
            return;
        }

        List<IBlockState> blockStates = World.Instance.GetOrAddBlockStates(chunkData.chunkCoord);

        foreach(IBlockState blockState in blockStates)
        {
            if(blockState.Position == worldPosition)
            {
                if (blockState is Furnace furnace)
                {
                    UIManager.Instance.OpenFurnace(furnace);
                    return;
                }
                else
                {
                    Debug.LogWarning($"Can't found furnace at {worldPosition}");
                }
            }
        }

        Furnace furnace1 = new Furnace(worldPosition);
        blockStates.Add(furnace1);
        UIManager.Instance.OpenFurnace(furnace1);
    }
}
