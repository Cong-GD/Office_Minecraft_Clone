using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static GameSettings;

public class TerrainGenerator : MonoBehaviour
{
    public float terrainScale;
    public float terrainOffset;

    public BiomeAttribute biome;

    public ChunkData GenerateChunk(World world, Vector3Int chunkCoord)
    {
        ChunkData chunkData = new ChunkData(world, chunkCoord);
        GameSettings.ChunkSize.Parse(out int xMax, out int yMax, out int zMax);

        Vector3Int pos = Vector3Int.zero;
        for (pos.x = 0; pos.x < xMax; pos.x++)
        {
            for (pos.z = 0; pos.z < zMax; pos.z++)
            {
                int noiseHeight = (int)(Noise.Get2DPerlin(new Vector2Int(chunkData.worldPosition.x + pos.x, chunkData.worldPosition.z + pos.z), 0 , biome.terrainScale) * biome.terrainHeight) + biome.solidGroundHeight;

                for (pos.y = 0; pos.y < yMax; pos.y++)
                {
                    var currentHeight = pos.y + chunkData.worldPosition.y;
                    if(currentHeight == 0)
                    {
                        chunkData.SetBlock(pos, BlockType.Bedrock);
                        continue;
                    }

                    BlockType blockToSet = BlockType.Air;

                    if (currentHeight == noiseHeight)
                    {
                        blockToSet = BlockType.GrassDirt;
                    }
                    else if (currentHeight > noiseHeight)
                    {
                        blockToSet = BlockType.Air;
                    }
                    else if (currentHeight > noiseHeight - 4)
                    {
                        blockToSet = BlockType.Dirt;
                    }
                    else
                    {
                        blockToSet = BlockType.Stone;
                    }

                    if(blockToSet == BlockType.Stone)
                    {
                        foreach (var lode in biome.lodes)
                        {
                            if (currentHeight < lode.minHeight || currentHeight > lode.maxHeight)
                                continue;

                            if(Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                            {
                                blockToSet = lode.blockType;
                            }
                        }
                    }


                    chunkData.SetBlock(pos, blockToSet);
                }
            }
        }
        chunkData.state = ChunkState.Generated;
        return chunkData;
    }
}