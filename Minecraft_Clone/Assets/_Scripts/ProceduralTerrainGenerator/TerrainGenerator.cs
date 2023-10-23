using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static GameSettings;

public class TerrainGenerator : MonoBehaviour
{
    public string seed;

    public BiomeGenerator biomeGenerator;

    private void Awake()
    {
        Random.InitState(seed.GetHashCode());
    }

    public ChunkData GenerateChunk(Vector3Int chunkCoord)
    {
        ChunkData chunkData = GetChunkData(chunkCoord);
        chunkData.state = ChunkState.Creating;
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int z = 0; z < CHUNK_WIDTH; z++)
            {
                biomeGenerator.ProcessChunkCollumn(chunkData, x, z);
            }
        }
        chunkData.state = ChunkState.Generated;
        return chunkData;
    }

    public ChunkData GetChunkData(Vector3Int chunkCoord)
    {
        var chunkData = ThreadSafePool<ChunkData>.Get();
        if (chunkData.state != ChunkState.InPool)
        {
            chunkData = new ChunkData();
        }
        chunkData.SetChunkCoord(chunkCoord);
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
}
