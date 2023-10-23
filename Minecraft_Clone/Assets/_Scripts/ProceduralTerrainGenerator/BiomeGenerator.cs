using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using static GameSettings;

public class BiomeGenerator : MonoBehaviour
{
    public int waterThreashold = 50;
    public float noiseScale = 0.03f;

    public NoiseSettings biomeNoiseSettings;
    public DomainWarping domainWarping;

    public bool useDomainWrapping;

    public BlockLayerHandler startLayerHandler;

    public List<BlockLayerHandler> addictionalLayerHandlers;

    public void ProcessChunkCollumn(ChunkData chunkData, int x, int z)
    {
        //Vector2Int worldPosition = new (chunkData.worldPosition.x + x, chunkData.worldPosition.z + z);
        int terrainHeight = GetSurfaceHeightNoise(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z);
        for (int y = 0; y < CHUNK_DEPTH; y++)
        {
            startLayerHandler.Handle(chunkData, x, y, z, terrainHeight); 
        }
        foreach (var layer in addictionalLayerHandlers)
        {
            layer.Handle(chunkData, x, 0, z, terrainHeight);
        }
    }

    private int GetSurfaceHeightNoise(int x, int z)
    {
        float terrainHeight;
        if (useDomainWrapping)
        {
            terrainHeight = domainWarping.GenerateDomainNoise(x, z, biomeNoiseSettings);
        }
        else
        {
            terrainHeight = Noise.OctavePerlin(x, z, biomeNoiseSettings);
        }
        terrainHeight = Noise.Redistribution(terrainHeight, biomeNoiseSettings);
        return Noise.RemapValue01ToInt(terrainHeight, 0, MAP_HEIGHT_IN_BLOCK);
    }
}
