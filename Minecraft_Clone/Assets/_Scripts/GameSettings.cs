using UnityEngine;

public static class GameSettings
{
    public static readonly Vector3Int ChunkSize = new Vector3Int(32, 32, 32);

    public static readonly int TotalBlockInChunk = ChunkSize.x * ChunkSize.y * ChunkSize.z;

    public static readonly int MapHeightInChunk = 4;

    public static readonly int MapHeightInBlock = MapHeightInChunk * ChunkSize.y;
}