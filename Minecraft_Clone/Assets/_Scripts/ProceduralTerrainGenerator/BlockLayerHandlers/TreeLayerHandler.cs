using Minecraft.ProceduralTerrain.Structures;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace Minecraft.ProceduralTerrain
{
    public class TreeLayerHandler : BlockLayerHandler
    {
        [SerializeField]
        private float terrainHeightLimit = 180;

        [SerializeField, Expandable]
        private NoiseGenerator_SO treeZoneNoiseGenerator;

        [SerializeField]
        private float treeZoneThreshold = 0.5f;

        [SerializeField, Expandable]
        private NoiseGenerator_SO treeNoiseGenerator;

        [SerializeField]
        private Structure_SO treeStructure;

        [SerializeField]
        private float localMaxRange = 1f;

        private NoiseInstance _treeZoneNoise;
        private NoiseInstance _treeNoise;

        private void Awake()
        {
            _treeZoneNoise = treeZoneNoiseGenerator.GetNoiseInstance();
            _treeNoise = treeNoiseGenerator.GetNoiseInstance();
        }

        protected override bool TryHandling(ChunkData chunkData, int x, int _, int z, int surfaceHeightNoise)
        {
            if (surfaceHeightNoise > terrainHeightLimit)
                return true;

            (int worldX, int worldY, int worldZ) = chunkData.worldPosition;
            worldX += x;
            worldZ += z;
            int localSurfaceHeight = surfaceHeightNoise - worldY;
            if (!Chunk.IsValidLocalY(localSurfaceHeight))
                return true;

            if (chunkData.GetBlock(x, localSurfaceHeight, z) != BlockType.GrassDirt)
                return true;

            if (_treeZoneNoise.GetNoise(worldX, worldZ) < treeZoneThreshold)
                return true;


            if (IsLocalMax(worldX, worldZ))
            {
                chunkData.SetBlock(x, localSurfaceHeight, z, BlockType.Dirt);
                Vector3Int treePosition = new Vector3Int(worldX, surfaceHeightNoise + 1, worldZ);
                chunkData.structures.Add((treePosition, treeStructure));
                return true;
            }

            return false;
        }

        private bool IsLocalMax(int x, int z)
        {
            var noiseValue = _treeNoise.GetNoise(x, z);

            if (_treeNoise.GetNoise(x + localMaxRange, z) > noiseValue)
                return false;

            if (_treeNoise.GetNoise(x, z + localMaxRange) > noiseValue)
                return false;

            if (_treeNoise.GetNoise(x - localMaxRange, z) > noiseValue)
                return false;

            if (_treeNoise.GetNoise(x, z - localMaxRange) > noiseValue)
                return false;

            return true;
        }

    }
}