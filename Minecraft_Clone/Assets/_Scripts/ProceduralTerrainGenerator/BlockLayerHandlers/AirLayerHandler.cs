namespace Minecraft.ProceduralTerrain
{
    public class AirLayerHandler : BlockLayerHandler
    {
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
        {
            if (chunkData.worldPosition.y + y > surfaceHeightNoise)
            {
                chunkData.SetBlock(x, y, z, BlockType.Air);
                return true;
            }
            return false;
        }
    }
}