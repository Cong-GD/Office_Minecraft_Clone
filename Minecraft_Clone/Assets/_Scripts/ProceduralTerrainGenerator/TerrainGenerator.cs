using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static GameSettings;

public class TerrainGenerator : MonoBehaviour
{
    public float terrainScale;
    public float terrainOffset;

    public BiomeAttribute biome;

    public Structure structure;

    public float terrainHeight;

    public ChunkData GenerateChunk(Vector3Int chunkCoord)
    {
        ChunkData chunkData = ConcurrentPool.GetChunkData(chunkCoord);
        chunkData.state = ChunkState.Creating;
        GameSettings.ChunkSize.Parse(out int xMax, out int yMax, out int zMax);
        chunkData.worldPosition.Parse(out int chunkX, out int chunkY, out int chunkZ);
        Vector3Int pos = Vector3Int.zero;
        for (pos.x = 0; pos.x < xMax; pos.x++)
        {
            for (pos.z = 0; pos.z < zMax; pos.z++)
            {
                int terrainHeight = (int)(Noise.Get2DPerlin(new Vector2Int(chunkX + pos.x, chunkZ + pos.z), 0 , biome.terrainScale) * biome.terrainHeight) + biome.solidGroundHeight;

                for (pos.y = 0; pos.y < yMax; pos.y++)
                {
                    var yPos = pos.y + chunkY;
                    if(yPos == 0)
                    {
                        chunkData.SetBlock(pos, BlockType.Bedrock);
                        continue;
                    }

                    BlockType blockToSet = BlockType.Air;

                    if (yPos == terrainHeight)
                    {
                        blockToSet = BlockType.GrassDirt;
                    }
                    else if (yPos > terrainHeight)
                    {
                        blockToSet = BlockType.Air;
                    }
                    else if (yPos > terrainHeight - 4)
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
                            if (yPos < lode.minHeight || yPos > lode.maxHeight)
                                continue;

                            if(Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                            {
                                blockToSet = lode.blockType;
                            }
                        }
                    }

                    if(yPos == terrainHeight)
                    {
                        if(Noise.Get2DPerlin(new Vector2Int(chunkX + pos.x, chunkZ + pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold)
                        {
                            if(Noise.Get2DPerlin(new Vector2Int(chunkX + pos.x, chunkZ + pos.z), 0, biome.treePlacementScale) > biome.treePlacementThreshold)
                            {
                                chunkData.structures.Add((chunkData.worldPosition + pos + Vector3Int.up, structure));
                            }
                        }
                    }


                    chunkData.SetBlock(ref pos, blockToSet);
                }
            }
        }
        chunkData.state = ChunkState.Generated;
        return chunkData;
    }

    public void ReleaseChunk(ChunkData chunkData)
    {
        ConcurrentPool.Release(chunkData);
    }
}
