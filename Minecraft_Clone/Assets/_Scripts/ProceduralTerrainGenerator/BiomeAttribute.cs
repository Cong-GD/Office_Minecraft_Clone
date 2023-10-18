using System;
using UnityEngine;

[System.Serializable]
public class Lode
{
    public string name;
    public BlockType blockType;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;
}

[CreateAssetMenu(menuName = "Minecraft/BiomeAttribute")]
public class BiomeAttribute : ScriptableObject
{
    public string BiomeName;

    public int solidGroundHeight;

    public int terrainHeight;

    public float terrainScale;

    [Header("Tree")]
    public float treeZoneScale = 1.3f;

    [Range(0.1f, 1f)]
    public float treeZoneThreshold = 0.6f;

    public float treePlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float treePlacementThreshold = 0.8f;

    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;

    public Lode[] lodes;
}
