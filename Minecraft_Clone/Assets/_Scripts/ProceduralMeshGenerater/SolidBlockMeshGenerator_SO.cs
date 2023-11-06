using System.Diagnostics;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Solid Cube Type Mesh")]
    public class SolidBlockMeshGenerator_SO : CubeTypeMeshGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int x, int y, int z)
        {
            for (int i = 0; i < FACES_COUNT; i++)
            {
                var adjacentBlockData = Chunk.GetBlock(chunkData,
                    x + sixDirectionVectors[i].x,
                    y + sixDirectionVectors[i].y,
                    z + sixDirectionVectors[i].z).Data();

                if (!adjacentBlockData.IsTransparent)
                    continue;

                VoxelHelper.AddQuadVertices(meshData.vertices, sixDirections[i], x, y, z);
                VoxelHelper.AddQuadUvs(meshData.uvs, GetUvIndex(sixDirections[i]));

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(sixDirectionVectors[i]);

                VoxelHelper.AddQuadTriangle(meshData.triangles, meshData.vertices.Count);

                if (!adjacentBlockData.IsSolid)
                    VoxelHelper.AddQuadTriangle(meshData.colliderTriangles, meshData.vertices.Count);
            }
        }
    }
}
