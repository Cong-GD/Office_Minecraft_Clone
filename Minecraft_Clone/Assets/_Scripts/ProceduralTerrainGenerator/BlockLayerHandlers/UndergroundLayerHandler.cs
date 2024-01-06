using NaughtyAttributes;
using UnityEngine;

namespace Minecraft.ProceduralTerrain
{
    [System.Serializable]
    public struct MineralThreshold
    {
        [Range(0, 1)]
        public float threshold;
        public int heightThreshold;
        public BlockType blockType;
    }


    public class UndergroundLayerHandler : BlockLayerHandler
    {
        //[SerializeField, Expandable]
        //private NoiseGenerator_SO caveNoiseGenerator;

        //[SerializeField]
        //private float caveThreshold = 0.3f;

        [SerializeField ,Expandable]
        private NoiseGenerator_SO mineralNoiseGenerator;

        [SerializeField]
        private MineralThreshold[] mineralThresholds;


        private NoiseInstance _mineralNoiseInstance;
        //private NoiseInstance _caveNoiseInstance;

        private void Awake()
        {
            _mineralNoiseInstance = mineralNoiseGenerator.GetNoiseInstance();
            //_caveNoiseInstance = caveNoiseGenerator.GetNoiseInstance();
        }

        protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
        {
            int worldY = chunkData.worldPosition.y + y;

            if (worldY < surfaceHeightNoise)
            {
                //float caveNoiseValue = _caveNoiseInstance.GetNoise(chunkData.worldPosition.x + x + 0.1f, worldY + 0.1f, chunkData.worldPosition.z + z + 0.1f);
                //if (caveNoiseValue < caveThreshold)
                //{
                //    chunkData.SetBlock(x, y, z, BlockType.Air);
                //    return false;
                //}

                float mineralNoiseValue = _mineralNoiseInstance.GetNoise(chunkData.worldPosition.x + x + 0.1f, worldY + 0.1f, chunkData.worldPosition.z + z + 0.1f);
                foreach (MineralThreshold mineralThreshold in mineralThresholds)
                {
                    if (mineralNoiseValue < mineralThreshold.threshold && worldY < mineralThreshold.heightThreshold)
                    {
                        chunkData.SetBlock(x, y, z, mineralThreshold.blockType);
                        return false;
                    }
                }
            }

            return false;
        }
    }
}