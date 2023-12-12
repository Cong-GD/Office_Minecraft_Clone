using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Glass Mesh")]
    public class GlassMeshGenerator_SO : CubeTypeMeshGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int localX, int localY, int localZ)
        {
            for (int i = 0; i < FACES_COUNT; i++)
            {
                Vector3Int directionVector = sixDirectionVectors[i];
                BlockData_SO adjacentBlockData = Chunk.GetBlock(chunkData,
                    localX + directionVector.x,
                    localY + directionVector.y,
                    localZ + directionVector.z).Data();

                if (!adjacentBlockData.IsTransparent)
                    continue;

                Direction direction = sixDirections[i];
                MeshDrawerHelper.AddQuadVertices(meshData.vertices, direction, localX, localY, localZ);

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(directionVector);

                MeshDrawerHelper.AddQuadTriangle(meshData.triangles, meshData.vertices.Count);

                if (!adjacentBlockData.IsSolid)
                    MeshDrawerHelper.AddQuadTriangle(meshData.colliderTriangles, meshData.vertices.Count);

                MeshDrawerHelper.AddQuadUvs(meshData.uvs, GetUvIndex(direction));

            }
        }
    }
}
