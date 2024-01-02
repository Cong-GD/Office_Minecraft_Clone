using Minecraft;
using System.Collections.Generic;
using UnityEngine;

public class FurnaceBlock_SO : BlockData_SO, IInteractableBlock
{
    public void Interact(Vector3Int worldPosition)
    {
        if (!World.Instance.TryGetChunkData(Chunk.GetChunkCoord(worldPosition), out var chunkData))
            throw new System.Exception($"Invalid furnace position {worldPosition}");

        if(chunkData.GetBlock(worldPosition - chunkData.worldPosition) != BlockType.Furnace)
            throw new System.Exception($"Can't found furnace at {worldPosition}");

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
                    throw new System.Exception($"Can't found furnace at {worldPosition}");
                }
            }
        }

        Furnace furnace1 = new Furnace(worldPosition);
        blockStates.Add(furnace1);
        UIManager.Instance.OpenFurnace(furnace1);
        return;
    }
}
