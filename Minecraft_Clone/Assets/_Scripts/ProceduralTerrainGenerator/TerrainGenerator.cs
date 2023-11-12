using Minecraft;
using Minecraft.ProceduralTerrain.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using static WorldSettings;
using UnityEngine.XR;
using NaughtyAttributes;

public class TerrainGenerator : MonoBehaviour
{
    [Serializable]
    public class BiomeData
    {
        [MinMaxSlider(0f, 1f)]
        public Vector2 temperatureThreshold;
        public BiomeGenerator biome;
    }

    [SerializeField] private NoiseGenerator_SO biomeTemperateNoiseGenerator;
    [SerializeField] private BiomeData[] biomeGeneratorsData = Array.Empty<BiomeData>();
    [SerializeField] private int waterLevel = 140;
    [SerializeField] private int biomeSelectRange = 2;
    [SerializeField] private int biomeSize = 150;
    private NoiseInstance _tempoNoiseInstance;

    private Dictionary<Vector2Int, BiomeGenerator> _biomeCenters = new Dictionary<Vector2Int, BiomeGenerator>();

    private void Awake()
    {
        WorldSettings.waterLevel = waterLevel;
        _tempoNoiseInstance = biomeTemperateNoiseGenerator.GetNoiseInstance();
    }

    public void CalculateBiomeCenter(float centerX, float centorZ)
    {
        lock (_biomeCenters)
        {
            _biomeCenters.Clear();
            int xOrigin = Mathf.RoundToInt(centerX / biomeSize) * biomeSize;
            int zOrigin = Mathf.RoundToInt(centorZ / biomeSize) * biomeSize;

            int startX = xOrigin - biomeSize * biomeSelectRange;
            int endX = xOrigin + biomeSize * biomeSelectRange + 1;
            int startZ = zOrigin - biomeSize * biomeSelectRange;
            int endZ = zOrigin + biomeSize * biomeSelectRange + 1;

            for (int x = startX; x < endX; x += biomeSize)
            {
                for (int z = startZ; z < endZ; z += biomeSize)
                {
                    _biomeCenters[new Vector2Int(x, z)] = SelectectBiome(x, z);
                }
            } 
        }
    }

    public ChunkData GenerateChunk(Vector3Int chunkCoord)
    {
        ChunkData chunkData = GetChunkData();
        chunkData.state = ChunkState.Creating;
        chunkData.SetChunkCoord(chunkCoord);
        var biomeDistances = ThreadSafePool<Dictionary<BiomeGenerator, float>>.Get();

        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                int worldX = chunkData.worldPosition.x + x;
                int worldZ = chunkData.worldPosition.z + z;
                var biome = SelectectBiome(worldX, worldZ);
                biomeDistances.Clear();
                GetBiomeDistance(biomeDistances, worldX, worldZ);
                float sum = biomeDistances.Values.Sum(v => v) / 2;

                float terrainHeight = 0f;
                foreach (var biomeDistance in biomeDistances)
                {
                    terrainHeight += biomeDistance.Key.GetSurfaceHeightNoise(worldX, worldZ) * ((sum - biomeDistance.Value) / sum);
                }

                if(terrainHeight >= MAP_HEIGHT_IN_BLOCK)
                {
                    Debug.Log($"{terrainHeight}, {sum}, {biomeDistances.Count}");
                }

                biome.ProcessChunkCollumn(chunkData, x, z, Mathf.RoundToInt(terrainHeight));
            }
        }

        biomeDistances.Clear();
        ThreadSafePool<Dictionary<BiomeGenerator, float>>.Release(biomeDistances);
        chunkData.state = ChunkState.Generated;
        return chunkData;
    }

    private BiomeGenerator SelectectBiome(int worldX, int worldZ)
    {
        float noiseValue = _tempoNoiseInstance.GetNoise(worldX, worldZ);
        foreach (var data in biomeGeneratorsData)
        {
            if (noiseValue >= data.temperatureThreshold.x && noiseValue < data.temperatureThreshold.y)
            {
                return data.biome;
            }
        }
        return biomeGeneratorsData[0].biome;
    }

    private void GetBiomeDistance(Dictionary<BiomeGenerator, float> biomeDistances, int x, int z)
    {
        var pos = new Vector2Int(x, z);

        foreach (var biomeCenter in _biomeCenters)
        {
            var distance = Vector2Int.Distance(biomeCenter.Key, pos);

            if (biomeDistances.TryGetValue(biomeCenter.Value, out var currentDistance))
            {
                if (currentDistance > distance)
                {
                    biomeDistances[biomeCenter.Value] = distance;
                }
            }
            else
            {
                biomeDistances[biomeCenter.Value] = distance;
            }
        }
    }


    public ChunkData GetChunkData()
    {
        var chunkData = ThreadSafePool<ChunkData>.Get();
        if (chunkData.state != ChunkState.InPool)
        {
            chunkData = new ChunkData();
        }

        return chunkData;
    }

    public void ReleaseChunk(ChunkData chunkData)
    {
        chunkData.state = ChunkState.InPool;
        chunkData.modifiedByPlayer = false;
        chunkData.isDirty = false;
        chunkData.structures.Clear();
        chunkData.modifierQueue.Clear();
        ThreadSafePool<ChunkData>.Release(chunkData);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _biomeCenters is null)
            return;

        Gizmos.color = Color.blue;
        lock (_biomeCenters)
        {
            foreach (var vec2 in _biomeCenters.Keys)
            {
                var pos = new Vector3(vec2.x, 0, vec2.y);
                Gizmos.DrawLine(pos, pos + Vector3.up * CHUNK_DEPTH);
            } 
        }
    }
#endif
}