namespace Minecraft.ProceduralTerrain
{
    public class BedrockLayerHandler : BlockLayerHandler
    {
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
        {
            int worldY = chunkData.worldPosition.y + y;
            if (worldY < 3)
            {
                if (worldY == 0)
                {
                    chunkData.SetBlock(x, y, z, BlockType.Bedrock);
                }
                else
                {
                    chunkData.SetBlock(x, y, z, BlockType.Stone);
                }
                return true;
            }
            return false;
        }
    }
}