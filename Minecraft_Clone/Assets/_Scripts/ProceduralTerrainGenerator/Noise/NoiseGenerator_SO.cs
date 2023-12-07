using NaughtyAttributes;
using System;
using UnityEditor;
using UnityEngine;

namespace Minecraft
{
    [CreateAssetMenu(menuName = "Minecraft/Precedural Terrain/Noise Generator")]
    public class NoiseGenerator_SO : ScriptableObject
    {
        public enum FractalType
        {
            None = 0,
            Fbm = 1,
            Ridged = 2,
            PingPong = 3,
        }

        public enum DomainFractalType
        {
            None = 0,
            DomainWarpProgressive = 4,
            DomainWarpIndependent = 5
        };


        [SerializeField]
        private NoiseSettings noiseSettings;

        [SerializeField]
        [Header("Domain Wrapping")]
        private bool useDomainWrapper;

        [SerializeField]
        [EnableIf("useDomainWrapper")]
        private DomainWrapper domainWrapper;

        [SerializeReference]
        [Tooltip("Add processors after noise value generated")]
        private INoisePostProcessor[] postProcessors = Array.Empty<INoisePostProcessor>();

        public NoiseInstance GetNoiseInstance()
        {
            NoiseInstance noiseInstance = new SingleNoise(noiseSettings.GetFastNoise());

            if (useDomainWrapper)
            {
                var domainNoise = domainWrapper.GetDomainWrapper();
                noiseInstance = new DomainWrappedNoise(noiseInstance, domainNoise);
            }

            if (postProcessors.Length > 0)
            {
                noiseInstance = new PostProcessedNoise(noiseInstance, postProcessors);
            }

            return noiseInstance;
        }

#if UNITY_EDITOR

        [Button]
        private void AddRedistributionProcessor()
        {
            ArrayUtility.Add(ref postProcessors, new RedistributePostProcess());
        }

        [Button]
        private void AddTerraceProcessor()
        {
            ArrayUtility.Add(ref postProcessors, new TerracesPostProcess());
        }

#endif

    }
}