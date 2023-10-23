public class WaterLayerHandler : BlockLayerHandler
{
    public int waterLevel = 1;
    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
    {
        if(y > surfaceHeightNoise && y <= waterLevel)
        {
            chunkData.SetBlock(x, y, z, BlockType.Water);
            if(y == surfaceHeightNoise + 1 && y + chunkData.worldPosition.y > 0)
            {
                chunkData.SetBlock(x, surfaceHeightNoise, z, BlockType.Sand);
            }
            return true;
        }
        return false;
    }
}
