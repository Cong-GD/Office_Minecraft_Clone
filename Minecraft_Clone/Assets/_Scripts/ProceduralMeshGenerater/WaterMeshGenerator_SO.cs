﻿using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Water Mesh")]
    public class WaterMeshGenerator_SO : CubeTypeMeshGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int localX, int localY, int localZ)
        {
            for (int i = 0; i < FACES_COUNT; i++)
            {
                var adjacentBlockData = Chunk.GetBlock(chunkData,
                    localX + sixDirectionVectors[i].x,
                    localY + sixDirectionVectors[i].y,
                    localZ + sixDirectionVectors[i].z).Data();

                if (adjacentBlockData.BlockType == BlockType.Water)
                    continue;

                if (adjacentBlockData.BlockType != BlockType.Glass && adjacentBlockData.IsSolid)
                    continue;

                MeshDrawerHelper.AddQuadVertices(meshData.vertices, sixDirections[i], localX, localY, localZ);

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(sixDirectionVectors[i]);

                MeshDrawerHelper.AddQuadTriangle(meshData.waterTriangles, meshData.vertices.Count);
                //MeshDrawerHelper.AddQuadUvs(meshData.uvs, GetUvIndex(sixDirections[i]));
                meshData.uvs.Add(new Vector2(0f, 0f));
                meshData.uvs.Add(new Vector2(0f, 1f));
                meshData.uvs.Add(new Vector2(1f, 1f));
                meshData.uvs.Add(new Vector2(1f, 0f));
            }
        }
    }
}
