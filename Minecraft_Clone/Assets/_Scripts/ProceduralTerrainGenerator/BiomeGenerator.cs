using Minecraft;
using Minecraft.ProceduralTerrain;
using System.Collections.Generic;
using UnityEngine;
using static WorldSettings;

public class BiomeGenerator : MonoBehaviour
{
    public NoiseGenerator_SO biomeNoiseGenerator;

    public bool useDomainWrapping;

    public BlockLayerHandler startLayerHandler;

    public List<BlockLayerHandler> addictionalLayerHandlers;

    public int solidGroundHeight = 128;
    public int terrainHeight = 50;

    private NoiseInstance _noiseInstance;

    private void Awake()
    {
        _noiseInstance = biomeNoiseGenerator.GetNoiseInstance();
    }

    public void ProcessChunkCollumn(ChunkData chunkData, int localX, int localZ, int terrainHeight)
    {
        for (int localY = 0; localY < CHUNK_DEPTH; localY++)
        {
            startLayerHandler.Handle(chunkData, localX, localY, localZ, terrainHeight);
        }
        foreach (var layer in addictionalLayerHandlers)
        {
            layer.Handle(chunkData, localX, 0, localZ, terrainHeight);
        }
    }

    public float GetSurfaceHeightNoise(int worldX, int worldZ)
    {
        float terrainHeightNoise;
        terrainHeightNoise = _noiseInstance.GetNoise(worldX, worldZ);
        return solidGroundHeight + terrainHeightNoise * terrainHeight;
    }
}
