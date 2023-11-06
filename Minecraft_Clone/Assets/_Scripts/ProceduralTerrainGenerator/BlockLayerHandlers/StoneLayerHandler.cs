using UnityEngine;

namespace Minecraft.ProceduralTerrain
{
    public class StoneLayerHandler : BlockLayerHandler
    {
        [Range(0, 1)]
        public float stoneThreashold = 0.5f;

        public NoiseGenerator_SO stoneNoise;

        private NoiseInstance _noiseInstance;

        private void Start()
        {
            _noiseInstance = stoneNoise.GetNoiseInstance();
        }

        protected override bool TryHandling(ChunkData chunkData, int x, int _, int z, int surfaceHeightNoise)
        {
            int localSurfaceHeight = surfaceHeightNoise - chunkData.worldPosition.y;
            if (!Chunk.IsValidLocalY(localSurfaceHeight))
                return false;

            float stoneNoiseValue = _noiseInstance.GetNoise(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z);

            if (stoneNoiseValue > stoneThreashold)
            {
                for (int i = 0; i <= localSurfaceHeight; i++)
                {
                    chunkData.SetBlock(x, i, z, BlockType.Stone);
                }
                return true;
            }
            return false;

        }
    }
}