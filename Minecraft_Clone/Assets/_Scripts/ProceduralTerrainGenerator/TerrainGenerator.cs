using Minecraft;
using NaughtyAttributes;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static WorldSettings;

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
        WorldSettings.WaterLevel = waterLevel;
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
            centerX = Mathf.RoundToInt((float)centerX / biomeSize) * biomeSize;
            centerZ = Mathf.RoundToInt((float)centerZ / biomeSize) * biomeSize;

            int startX = centerX - biomeSize * biomeSelectRange;
            int startZ = centerZ - biomeSize * biomeSelectRange;

            Span<bool> flags = stackalloc bool[_totalBiomes];

            for (int x = 0; x < _gridSize; x++)
            {
                for (int z = 0; z < _gridSize; z++)
                {
                    int worldX = x * biomeSize + startX;
                    int worldZ = z * biomeSize + startZ;
                    int id = SelectBiomeId(worldX, worldZ);
                    flags[id] = true;
                    int index = z * _gridSize + x;
                    _biomePositions[index] = new Vector2(worldX, worldZ);
                    _biomeIndexes[index] = id;
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
        int chunkX = chunkData.worldPosition.x;
        int chunkZ = chunkData.worldPosition.z;
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                int worldX = chunkX + x;
                int worldZ = chunkZ + z;
                BiomeGenerator biome = Biome(SelectBiomeId(worldX, worldZ));
                Span<float> distances = stackalloc float[_totalBiomes];
                GetBiomeDistance(distances, worldX, worldZ, out var sum);
                float sufaceHeight = 0f;

                for (int i = 0; i < distances.Length; i++)
                {
                    float distance = distances[i];
                    bool isBiomeInRange = distance > 0f;
                    if (isBiomeInRange)
                    {
                        float weight = 1f - distance / sum;
                        float unitSufaceHeight = Biome(i).GetSurfaceHeightNoise(worldX, worldZ);
                        sufaceHeight += unitSufaceHeight * weight;
                    }
                }
                biome.ProcessChunkCollumn(chunkData, x, z, Mathf.RoundToInt(sufaceHeight));
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

        for (int i = 0; i < biomeGeneratorsData.Length; i++)
        {
            if (biomeGeneratorsData[i].IsSuit(noiseValue))
                return i;
        }
        return 0;
    }

    private void GetBiomeDistance(Span<float> distances, int x, int z, out float sum)
    {
        Span<int> biomeIndexes = _biomeIndexes;
        Span<Vector2> biomePosition = _biomePositions;

        var position = new Vector2(x + 0.1f, z + 0.1f);
        for (int i = 0; i < biomeIndexes.Length; i++)
        {
            int biomeId = biomeIndexes[i];
            ref float currentDistance = ref distances[biomeId];
            float distance = Vector2.Distance(position, biomePosition[i]);
            if (currentDistance == 0f || distance < currentDistance)
            {
                currentDistance = distance;
            }
        }
        sum = 0f;
        for (int i = 0; i < distances.Length; i++)
            sum += distances[i];

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
        chunkData.isDirty = false;
        chunkData.structures.Clear();
        chunkData.modifierQueue.Clear();
        ThreadSafePool<ChunkData>.Release(chunkData);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _biomePositions is null)
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