using Minecraft.ProceduralTerrain.Structures;
using UnityEngine;

namespace Minecraft.ProceduralTerrain
{
    public class TreeLayerHandler : BlockLayerHandler
    {
        public float terrainHeightLimit = 25;

        public NoiseGenerator_SO treeZoneNoiseGenerator;
        public float treeZoneThreshold = 0.5f;

        public NoiseGenerator_SO treeNoiseGenerator;

        public Structure_SO treeStructure;

        public float treeThreshold = 0.8f;

        private NoiseInstance _treeZoneNoise;
        private NoiseInstance _treeNoise;

        private void Awake()
        {
            _treeZoneNoise = treeZoneNoiseGenerator.GetNoiseInstance();
            _treeNoise = treeNoiseGenerator.GetNoiseInstance();
        }

        protected override bool TryHandling(ChunkData chunkData, int x, int _, int z, int surfaceHeightNoise)
        {
            chunkData.worldPosition.Parse(out var worldX, out var worldY, out var worldZ);
            worldX += x;
            worldZ += z;
            int localSurfaceHeight = surfaceHeightNoise - worldY;
            if (!Chunk.IsValidLocalY(localSurfaceHeight))
                return true;

            if (surfaceHeightNoise > terrainHeightLimit)
                return true;

            if (chunkData.GetBlock(x, localSurfaceHeight, z) != BlockType.GrassDirt)
                return true;

            if (_treeZoneNoise.GetNoise(worldX, worldZ) < treeZoneThreshold)
                return true;

            if (_treeNoise.GetNoise(worldX, worldZ) > treeThreshold)
            {
                chunkData.SetBlock(x, localSurfaceHeight, z, BlockType.Dirt);
                chunkData.structures.Add((new Vector3Int(worldX, surfaceHeightNoise + 1, worldZ), treeStructure));
                return true;
            }

            return false;
        }

    }
}