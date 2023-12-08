using System;
using UnityEngine;

namespace Minecraft
{
    [Serializable]
    public class TerracesPostProcess : INoisePostProcessor
    {
        public string name = "Terraces";

        [SerializeField, Range(4, 32)]
        private int terraces;

        public float Process(float noiseValue)
        {
            return Mathf.Round(noiseValue * terraces) / terraces;
        }
    }
}