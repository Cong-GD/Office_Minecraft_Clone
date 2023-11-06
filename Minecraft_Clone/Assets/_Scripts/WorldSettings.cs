using UnityEngine;

public class WorldSettings : ScriptableObject
{
    public const int CHUNK_WIDTH = 16;

    public const int CHUNK_DEPTH = 128;

    public const int MAP_HEIGHT_IN_CHUNK = 2;

    public const int MAP_HEIGHT_IN_BLOCK = MAP_HEIGHT_IN_CHUNK * CHUNK_DEPTH;

    public const int TOTAL_BLOCK_IN_CHUNK = CHUNK_WIDTH * CHUNK_DEPTH * CHUNK_WIDTH;

    public static readonly Vector3Int ChunkSizeVector = new (CHUNK_WIDTH, CHUNK_DEPTH, CHUNK_WIDTH);

    public static int worldSeed;

    public static int waterLevel;

    public int viewDistance = 1;
    public int ChunkDataLoadRange => viewDistance + 1;
    public int HiddenChunkDistance => viewDistance + 2;
}

