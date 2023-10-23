public class SurfaceLayerHandler : BlockLayerHandler
{
    public BlockType surfaceBLockType;
    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
    {
        if(y == surfaceHeightNoise)
        {
            chunkData.SetBlock(x, y, z, surfaceBLockType);
            return true;
        }
        return false;
    }
}