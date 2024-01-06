using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    [CreateAssetMenu(fileName = "BarrelBlock", menuName = "Minecraft/Item/Block/BarrelBlock")]
    public class BarrelBlock_SO : BlockData_SO, IInteractableBlock
    {
        public void Interact(Vector3Int worldPosition)
        {
            if (!World.Instance.TryGetChunkData(Chunk.GetChunkCoord(worldPosition), out var chunkData))
            {
                Debug.LogWarning($"Invalid barrel position {worldPosition}");
                return;
            }

            if (chunkData.GetBlock(worldPosition - chunkData.worldPosition) != BlockType.Barrel)
            {
                Debug.LogWarning($"Can't found barrel at {worldPosition}");
                return;
            }

            List<IBlockState> blockStates = World.Instance.GetOrAddBlockStates(chunkData.chunkCoord);

            foreach (IBlockState blockState in blockStates)
            {
                if (blockState.Position == worldPosition)
                {
                    if (blockState is Stogare stogare)
                    {
                        UIManager.Instance.OpenStogare(stogare);
                        return;
                    }
                    else
                    {
                        Debug.LogWarning($"Can't found barrel at {worldPosition}");
                    }
                }
            }

            Stogare storage = new Stogare(worldPosition);
            blockStates.Add(storage);
            UIManager.Instance.OpenStogare(storage);
        }
    }
}