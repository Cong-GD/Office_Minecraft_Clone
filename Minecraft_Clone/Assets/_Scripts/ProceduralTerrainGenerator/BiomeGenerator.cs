using Minecraft;
using Minecraft.ProceduralTerrain;
using System;
using UnityEngine;
using static WorldSettings;

public class BiomeGenerator : MonoBehaviour
{
    public NoiseGenerator_SO biomeNoiseGenerator;

    public bool useDomainWrapping;

    public BlockLayerHandler startLayerHandler;

    public BlockLayerHandler[] addictionalLayerHandlers = Array.Empty<BlockLayerHandler>();

    public int solidGroundHeight = 128;
    public int terrainHeight = 50;

    private NoiseInstance _noiseInstance;

    private void Awake()
    {
        _noiseInstance = biomeNoiseGenerator.GetNoiseInstance();
    }

    public void ProcessChunkCollumn(ChunkData chunkData, int localX, int localZ, int sufaceHeight)
    {
        BlockLayerHandler startLayer = startLayerHandler;
        for (int localY = 0; localY < CHUNK_DEPTH; localY++)
        {
            startLayer.Handle(chunkData, localX, localY, localZ, sufaceHeight);
        }
        foreach (BlockLayerHandler layer in addictionalLayerHandlers)
        {
            layer.Handle(chunkData, localX, 0, localZ, sufaceHeight);
        }
    }

    public float GetSurfaceHeightNoise(int worldX, int worldZ)
    {
        float terrainHeightNoise = _noiseInstance.GetNoise(worldX, worldZ);
        return solidGroundHeight + terrainHeightNoise * terrainHeight;
    }
}
