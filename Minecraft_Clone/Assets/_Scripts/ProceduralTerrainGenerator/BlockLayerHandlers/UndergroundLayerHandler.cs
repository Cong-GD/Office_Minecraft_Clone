public class UndergroundLayerHandler : BlockLayerHandler
{
    public BlockType underGroundBlockType;

    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
    {
        if(y < surfaceHeightNoise)
        {
            chunkData.SetBlock(x, y, z, underGroundBlockType);
            return true;
        }
        return false;
    }
}
