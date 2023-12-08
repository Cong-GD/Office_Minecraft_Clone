using System;
using UnityEngine;

namespace Minecraft
{
    [Serializable]
    public class RedistributePostProcess : INoisePostProcessor
    {
        public string name = "Redistribute";

        [SerializeField, Range(0f, 10f)]
        private float exponent;

        public float Process(float noiseValue)
        {
            return Mathf.Pow(noiseValue, exponent);
        }
    }
}