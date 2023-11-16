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
using System.Runtime.CompilerServices;

public class TerrainGenerator : MonoBehaviour
{
    [Serializable]
    public class BiomeData
    {
        [MinMaxSlider(0f, 1f)]
        public Vector2 temperatureThreshold;
        public BiomeGenerator biome;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSuit(float noiseValue)
        {
            return noiseValue >= temperatureThreshold.x && noiseValue < temperatureThreshold.y;
        }
    }

    [SerializeField] private NoiseGenerator_SO biomeTemperateNoiseGenerator;
    [SerializeField] private BiomeData[] biomeGeneratorsData = Array.Empty<BiomeData>();
    [SerializeField] private int waterLevel = 140;
    [SerializeField] private int biomeSelectRange = 2;
    [SerializeField] private int biomeSize = 150;

    private NoiseInstance _tempoNoiseInstance;
    private int _totalBiomes;
    private int _nearBiomeCount;
    private BiomeGenerator[] _biomeLookupArray;

    private Vector2[] _biomePositions;
    private int[] _biomeIndexes;
    private int _gridSize;
    private object _biomeCalculationThreadLock = new object();
    

    private void Awake()
    {
        WorldSettings.waterLevel = waterLevel;
        _tempoNoiseInstance = biomeTemperateNoiseGenerator.GetNoiseInstance();
        _totalBiomes = biomeGeneratorsData.Length;
        _biomeLookupArray = biomeGeneratorsData.Select(data => data.biome).ToArray();

        _gridSize = biomeSelectRange * 2 + 1;
        _biomePositions = new Vector2[_gridSize * _gridSize];
        _biomeIndexes = new int[_gridSize * _gridSize];
    }

    public void CalculateBiomeCenter(int centerX, int centerZ)
    {
        lock (_biomeCalculationThreadLock)
        {
            var xCenter = Mathf.RoundToInt((float)centerX / biomeSize) * biomeSize;
            var zCenter = Mathf.RoundToInt((float)centerZ / biomeSize) * biomeSize;

            int startX = xCenter - biomeSize * biomeSelectRange;
            int startZ = zCenter - biomeSize * biomeSelectRange;

            Span<bool> flags = stackalloc bool[_totalBiomes];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int z = 0; z < _gridSize; z++)
                {
                    int worldX = x * biomeSize + startX;
                    int worldZ = z * biomeSize + startZ;
                    int id = SelectBiomeId(worldX, worldZ);
                    flags[id] = true;
                    _biomePositions[z * _gridSize + x] = new Vector2(worldX, worldZ);
                    _biomeIndexes[z * _gridSize + x] = id;
                }
            }
            _nearBiomeCount = 0;
            foreach (var flag in flags)
            {
                _nearBiomeCount += flag ? 1 : 0;
            }
        }
    }

    public ChunkData GenerateChunk(Vector3Int chunkCoord)
    {
        ChunkData chunkData = GetChunkData();
        chunkData.state = ChunkState.Creating;
        chunkData.SetChunkCoord(chunkCoord);

        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                int worldX = chunkData.worldPosition.x + x;
                int worldZ = chunkData.worldPosition.z + z;
                var biome = Biome(SelectBiomeId(worldX, worldZ));
                Span<float> distances = stackalloc float[_totalBiomes];
                GetBiomeDistance(distances, worldX, worldZ, out var sum);
                float terrainHeight = 0f;

                for (int i = 0; i < _totalBiomes; i++)
                {
                    var distance = distances[i];
                    if (distance > 0f)
                    {
                        float weight = 1f - distance / sum;
                        float surfaceHeight = Biome(i).GetSurfaceHeightNoise(worldX, worldZ);
                        terrainHeight += surfaceHeight * weight;
                    }
                }

                biome.ProcessChunkCollumn(chunkData, x, z, Mathf.RoundToInt(terrainHeight));
            }
        }
        chunkData.state = ChunkState.Generated;
        return chunkData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BiomeGenerator Biome(int biomeId)
    {
        return _biomeLookupArray[biomeId];
    }

    private int SelectBiomeId(int worldX, int worldZ)
    {
        float noiseValue = _tempoNoiseInstance.GetNoise(worldX, worldZ);

        for (int i = 0; i < _totalBiomes; i++)
        {
            if (biomeGeneratorsData[i].IsSuit(noiseValue))
                return i;
        }
        return 0;
    }

    private void GetBiomeDistance(Span<float> distances, int x, int z, out float sum)
    {
        var pos = new Vector2(x + 0.1f, z + 0.1f);
        for (int i = 0; i < _biomeIndexes.Length; i++)
        {
            int biomeId = _biomeIndexes[i];
            float distance = Vector2.Distance(pos, _biomePositions[i]);
            if (distances[biomeId] == 0f || distance < distances[biomeId])
            {
                distances[biomeId] = distance;
            }
        }
        sum = 0f;
        for (int i = 0; i < _totalBiomes; i++)
        {
            sum += distances[i];
        }
        sum /= 2f;
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
        if (!Application.isPlaying || _biomeIndexes is null)
            return;

        Gizmos.color = Color.blue;
        lock (_biomeCalculationThreadLock)
        {
            foreach (var vec2 in _biomePositions)
            {
                var pos = new Vector3(vec2.x, 0, vec2.y);
                Gizmos.DrawLine(pos, pos + Vector3.up * CHUNK_DEPTH);
            } 
        }
    }
#endif
}