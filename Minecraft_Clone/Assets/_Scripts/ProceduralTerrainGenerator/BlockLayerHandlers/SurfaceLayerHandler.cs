namespace Minecraft.ProceduralTerrain
{
    public class SurfaceLayerHandler : BlockLayerHandler
    {
        public BlockType surfaceBLockType;
        public BlockType underSufaceBlockType;
        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeight)
        {
            int localSufaceHeight = surfaceHeight - chunkData.worldPosition.y;
            if(!Chunk.IsValidLocalY(localSufaceHeight))
                return false;

            if (y == localSufaceHeight)
            {
                chunkData.SetBlock(x, y, z, surfaceBLockType);
                return true;
            }
            else if (y < localSufaceHeight && y > localSufaceHeight - 5)
            {
                chunkData.SetBlock(x, y, z, underSufaceBlockType);
                return true;
            }
            return false;
        }
    }
}