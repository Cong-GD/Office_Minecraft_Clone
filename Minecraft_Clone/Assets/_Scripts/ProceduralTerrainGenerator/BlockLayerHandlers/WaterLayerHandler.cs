namespace Minecraft.ProceduralTerrain
{
    public class WaterLayerHandler : BlockLayerHandler
    {
        public int waterLevel = 1;
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
        {
            int yPos = chunkData.worldPosition.y + y;
            if (yPos > surfaceHeightNoise && yPos <= WorldSettings.WaterLevel)
            {
                chunkData.SetBlock(x, y, z, BlockType.Water);
                if (yPos == surfaceHeightNoise + 1 && y > 0)
                {
                    chunkData.SetBlock(x, y - 1, z, BlockType.Sand);
                }
                return true;
            }
            return false;
        }
    }
}