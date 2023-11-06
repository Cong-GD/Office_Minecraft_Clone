using UnityEngine;

namespace Minecraft
{
    [CreateAssetMenu(menuName = "Minecraft/Precedural Terrain/Post Processor/Redistribute")]
    public class RedistributeProcess_SO : NoisePostProcess_SO
    {
        [SerializeField, Range(0f, 10f)] 
        private float exponent;

        public override float Process(float noiseValue)
        {
            return Mathf.Pow(noiseValue, exponent);
        }
    }
}