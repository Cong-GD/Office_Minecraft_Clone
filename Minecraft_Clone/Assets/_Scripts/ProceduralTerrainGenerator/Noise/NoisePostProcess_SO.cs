using UnityEngine;

namespace Minecraft
{
    public abstract class NoisePostProcess_SO : ScriptableObject
    {
        public abstract float Process(float noiseValue);

    }
}