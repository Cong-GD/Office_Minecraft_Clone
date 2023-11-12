using Minecraft;
using UnityEngine;

public class FurnaceBlock_SO : BlockData_SO, IInteractable
{
    public void Interact(Vector3Int worldPosition)
    {
        if (!World.Instance.TryGetChunkData(Chunk.GetChunkCoord(worldPosition), out var chunkData))
            throw new System.Exception($"Invalid furnace position {worldPosition}");

        if(chunkData.GetBlock(worldPosition - chunkData.worldPosition) != BlockType.Furnace)
            throw new System.Exception($"Can't found furnace at {worldPosition}");

        if (!chunkData.blockStates.TryGetValue(worldPosition, out var blockState))
        {
            blockState = new BlastFurnace(worldPosition);
            chunkData.blockStates[worldPosition] = blockState;
        }

        if (blockState is not BlastFurnace furnace)
            throw new System.Exception($"Unexpected behaviour when open furnace at {worldPosition}");

        UIManager.Instance.OpenBlastFurnace(furnace);
        return;
    }
}
