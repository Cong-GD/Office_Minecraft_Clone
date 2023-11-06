using Minecraft.ProceduralTerrain;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.VisualScripting;
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

        [SerializeField]
        [Expandable] 
        [Tooltip("Add processors after noise value generated")]
        private NoisePostProcess_SO[] postProcessors = Array.Empty<NoisePostProcess_SO>();

        public NoiseInstance GetNoiseInstance()
        {
            NoiseInstance noiseInstance = new SingleNoise(noiseSettings.GetFastNoise());

            if (useDomainWrapper)
            {
                var domainNoise = domainWrapper.GetDomainWrapper();
                noiseInstance = new DomainWrappedNoise(noiseInstance, domainNoise);
            }

            if(postProcessors.Length > 0)
            {
                noiseInstance = new PostProcessedNoise(noiseInstance, postProcessors);
            }

            return noiseInstance;
        }
    }
}