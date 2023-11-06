using UnityEngine;

namespace Minecraft
{
    [CreateAssetMenu(menuName = "Minecraft/Precedural Terrain/Post Processor/Terraces")]
    public class TerracesProcess_SO : NoisePostProcess_SO
    {
        [SerializeField, Range(4, 32)] 
        private int terraces;

        public override float Process(float noiseValue)
        {
            return Mathf.Round(noiseValue * terraces) / terraces;
        }
    }
}