namespace Minecraft.ProceduralTerrain
{
    public class UndergroundLayerHandler : BlockLayerHandler
    {
        public BlockType underGroundBlockType;

        public float caveThreashold = 0.5f;

        public NoiseGenerator_SO caveNoiseGenerator;

        private NoiseInstance _caveNoiseInstance;

        private void Awake()
        {
            _caveNoiseInstance = caveNoiseGenerator.GetNoiseInstance();
        }

        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
        {
            int worldY = chunkData.worldPosition.y + y;

            if (worldY < surfaceHeightNoise)
            {
                //var caveNoiseValue = _caveNoiseInstance.GetNoise(chunkData.worldPosition.x + x +0.1f, worldY+0.1f, chunkData.worldPosition.z + z + 0.1f);
                //if (caveNoiseValue > caveThreashold)
                //{
                //    chunkData.SetBlock(x, y, z, BlockType.Air);
                //}
                //else
                //{
                //    chunkData.SetBlock(x, y, z, BlockType.Stone);
                //}
                //return false;
                chunkData.SetBlock(x, y, z, underGroundBlockType);
                return true;
            }
            return false;
        }
    }
}