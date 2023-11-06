﻿using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Glass Mesh")]
    public class GlassMeshGenerator_SO : CubeTypeMeshGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int localX, int localY, int localZ)
        {
            for (int i = 0; i < FACES_COUNT; i++)
            {
                var adjacentBlockData = Chunk.GetBlock(chunkData,
                    localX + sixDirectionVectors[i].x,
                    localY + sixDirectionVectors[i].y,
                    localZ + sixDirectionVectors[i].z).Data();

                if (!adjacentBlockData.IsTransparent || adjacentBlockData.BlockType == BlockType.Glass)
                    continue;

                VoxelHelper.AddQuadVertices(meshData.vertices, sixDirections[i], localX, localY, localZ);

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(sixDirectionVectors[i]);

                VoxelHelper.AddQuadTriangle(meshData.transparentTriangles, meshData.vertices.Count);

                if (!adjacentBlockData.IsSolid)
                    VoxelHelper.AddQuadTriangle(meshData.colliderTriangles, meshData.vertices.Count);

                VoxelHelper.AddQuadUvs(meshData.uvs, GetUvIndex(sixDirections[i]));

            }
        }
    }
}