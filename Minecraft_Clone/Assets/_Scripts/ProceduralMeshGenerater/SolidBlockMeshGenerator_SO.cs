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
                Vector3Int directionVector = sixDirectionVectors[i];
                BlockData_SO adjacentBlockData = Chunk.GetBlock(chunkData,
                    x + directionVector.x,
                    y + directionVector.y,
                    z + directionVector.z).Data();

                if (!adjacentBlockData.IsTransparent)
                    continue;

                Direction direction = sixDirections[i];
                MeshDrawerHelper.AddQuadVertices(meshData.vertices, direction, x, y, z);
                MeshDrawerHelper.AddQuadUvs(meshData.uvs, GetUvIndex(direction));

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(directionVector);

                MeshDrawerHelper.AddQuadTriangle(meshData.triangles, meshData.vertices.Count);

                if (!adjacentBlockData.IsSolid)
                    MeshDrawerHelper.AddQuadTriangle(meshData.colliderTriangles, meshData.vertices.Count);
            }
        }
    }
}
