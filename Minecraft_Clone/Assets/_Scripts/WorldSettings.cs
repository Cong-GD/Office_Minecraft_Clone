using UnityEngine;

public class WorldSettings
{
    public const int CHUNK_WIDTH = 16;

    public const int CHUNK_DEPTH = 128;

    public const int MAP_HEIGHT_IN_CHUNK = 2;

    public const int MAP_HEIGHT_IN_BLOCK = MAP_HEIGHT_IN_CHUNK * CHUNK_DEPTH;

    public const int TOTAL_BLOCK_IN_CHUNK = CHUNK_WIDTH * CHUNK_DEPTH * CHUNK_WIDTH;

    public static readonly Vector3Int ChunkSizeVector = new (CHUNK_WIDTH, CHUNK_DEPTH, CHUNK_WIDTH);

    public static int WorldSeed { get; set; }

    public static int WaterLevel { get; set;}

    public static int ChunkRenderRange { get; set; }
}
