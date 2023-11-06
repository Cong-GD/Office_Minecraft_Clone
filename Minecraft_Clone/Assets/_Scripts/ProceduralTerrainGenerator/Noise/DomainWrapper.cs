using JetBrains.Annotations;
using NaughtyAttributes;

namespace Minecraft
{
    [System.Serializable]
    public class DomainWrapper
    {
        public enum DomainFractalType
        {
            None = 0,
            DomainWarpProgressive = 4,
            DomainWarpIndependent = 5
        };
        public int seed;
        public FastNoise.DomainWarpType domainWrappingType;
        public DomainFractalType domainFractalType;
        public float frequency = 0.01f;

        public float amplitude = 1.0f;

        [DisableIf("domainFractalType", DomainFractalType.None)]
        [AllowNesting]
        public int octaves = 3;

        [DisableIf("domainFractalType", DomainFractalType.None)]
        [AllowNesting]
        public float gain = 0.5f;

        [DisableIf("domainFractalType", DomainFractalType.None)]
        [AllowNesting]
        public float lucunarity = 2f;

        public FastNoise GetDomainWrapper()
        {
            FastNoise fastNoise = new FastNoise(World.WorldSeed + seed);
            fastNoise.SetFractalType((FastNoise.FractalType)(int)domainWrappingType);
            fastNoise.SetDomainWarpType(domainWrappingType);
            fastNoise.SetFrequency(frequency);
            fastNoise.SetDomainWarpAmp(amplitude);
            fastNoise.SetFractalOctaves(octaves);
            fastNoise.SetFractalGain(gain);
            fastNoise.SetFractalLacunarity(lucunarity);
            return fastNoise;
        }
    }
}