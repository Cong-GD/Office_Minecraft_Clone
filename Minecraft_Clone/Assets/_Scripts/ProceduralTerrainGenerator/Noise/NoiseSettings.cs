using NaughtyAttributes;
using System;
using UnityEngine;
using static Minecraft.NoiseGenerator_SO;

namespace Minecraft
{
    [Serializable]
    public class NoiseSettings
    {
        [Serializable]
        public class CelcularValue
        {
            public FastNoise.CellularDistanceFunction cellularDistanceFunction;
            public FastNoise.CellularReturnType cellularReturnType;
            public float cellularJitterModifier = 1.0f;
        }

        public int seedOffset;
        public FastNoise.NoiseType noiseType;
        public FastNoise.RotationType3D rotationType3d;

        [MinValue(0.0001f)]
        [Tooltip("Never let this value a whole number\nIt's will produce the same noise every where")]
        public float frequency = 0.01f;

        [Tooltip("Apply when noise type is Cellcular")]
        [EnableIf("noiseType", FastNoise.NoiseType.Cellular)]
        [AllowNesting]
        public CelcularValue cellularField = new();

        [Header("Fractal parameters")]
        public FractalType fractalType;

        [DisableIf("fractalType", FractalType.None)]
        [AllowNesting]
        public int octaves = 5;

        [DisableIf("fractalType", FractalType.None)]
        [AllowNesting]
        public float gain = 0.5f;

        [DisableIf("fractalType", FractalType.None)]
        [AllowNesting]
        public float lucunarity = 2f;

        [DisableIf("fractalType", FractalType.None)]
        [AllowNesting]
        public float weightedStrength = 0f;

        [EnableIf("fractalType", FractalType.PingPong)]
        [AllowNesting]
        public float pingpongStrength = 2f;

        public FastNoise GetFastNoise()
        {
            FastNoise fastNoise = new FastNoise(World.WorldSeed + seedOffset);
            fastNoise.SetNoiseType(noiseType);

            if(noiseType == FastNoise.NoiseType.Cellular)
            {
                fastNoise.SetCellularDistanceFunction(cellularField.cellularDistanceFunction);
                fastNoise.SetCellularReturnType(cellularField.cellularReturnType);
                fastNoise.SetCellularJitter(cellularField.cellularJitterModifier);
            }
            
            fastNoise.SetFractalType((FastNoise.FractalType)(int)fractalType);
            fastNoise.SetRotationType3D(rotationType3d);
            fastNoise.SetFrequency(frequency);
            fastNoise.SetFractalOctaves(octaves);
            fastNoise.SetFractalGain(gain);
            fastNoise.SetFractalLacunarity(lucunarity);
            fastNoise.SetFractalWeightedStrength(weightedStrength);
            fastNoise.SetFractalPingPongStrength(pingpongStrength);
            return fastNoise;
        }

    }
}

