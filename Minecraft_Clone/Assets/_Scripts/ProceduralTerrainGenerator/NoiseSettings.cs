using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Noise Settings", menuName = "Minecraft/Data/NoiseSettings")]
public class NoiseSettings : ScriptableObject
{
    public float noiseZoom;
    public int octaves;
    public Vector2Int offest;
    public float persistance;
    public float redistributionModifier;
    public float exponent;
}
