using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Solid Facing Cube Type Mesh")]
    public class SolidFacingBlockMeshGenerator_SO : CubeTypeMeshGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int x, int y, int z)
        {
            var face = chunkData.GetDirection(x, y, z);
            for (int i = 0; i < FACES_COUNT; i++)
            {
                var adjacentBlockData = Chunk.GetBlock(chunkData,
                    x + sixDirectionVectors[i].x,
                    y + sixDirectionVectors[i].y,
                    z + sixDirectionVectors[i].z).Data();

                if (!adjacentBlockData.IsTransparent)
                    continue;

                MeshDrawerHelper.AddQuadVertices(meshData.vertices, sixDirections[i], x, y, z);
                MeshDrawerHelper.AddQuadUvs(meshData.uvs, GetUvIndex(sixDirections[i], face));

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(sixDirectionVectors[i]);

                MeshDrawerHelper.AddQuadTriangle(meshData.triangles, meshData.vertices.Count);

                if (!adjacentBlockData.IsSolid)
                    MeshDrawerHelper.AddQuadTriangle(meshData.colliderTriangles, meshData.vertices.Count);
            }
        }
    }
}
