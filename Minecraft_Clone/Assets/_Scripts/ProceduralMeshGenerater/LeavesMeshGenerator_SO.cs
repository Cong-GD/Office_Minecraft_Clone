using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Leaves Mesh")]
    public class LeavesMeshGenerator_SO : CubeTypeMeshGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int x, int y, int z)
        {
            //bool isTransparent = IsLeavesTransparent(chunkData, x, y, z);

            for (int i = 0; i < FACES_COUNT; i++)
            {
                int nextX = x + sixDirectionVectors[i].x;
                int nextY = y + sixDirectionVectors[i].y;
                int nextZ = z + sixDirectionVectors[i].z;
                var nextBlockData = Chunk.GetBlock(chunkData, nextX, nextY, nextZ).Data();

                if (!nextBlockData.IsTransparent)
                    continue;

                //if (nextBlockData.BlockType == BlockType.Leaves)
                //{
                //    bool isNextTransparent = IsLeavesTransparent(chunkData, nextX, nextY, nextZ);
                //    if (isTransparent && isNextTransparent || !isNextTransparent)
                //        continue;

                //}

                MeshDrawerHelper.AddQuadVertices(meshData.vertices, sixDirections[i], x, y, z);

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(sixDirectionVectors[i]);

                //if (isTransparent)
                //{
                //    VoxelHelper.AddQuadTriangle(meshData.transparentTriangles, meshData.vertices.Count);
                //}
                //else
                //{
                //    VoxelHelper.AddQuadTriangle(meshData.triangles, meshData.vertices.Count);
                //}

                MeshDrawerHelper.AddQuadTriangle(meshData.triangles, meshData.vertices.Count);

                if (!nextBlockData.IsSolid)
                    MeshDrawerHelper.AddQuadTriangle(meshData.colliderTriangles, meshData.vertices.Count);

                MeshDrawerHelper.AddQuadUvs(meshData.uvs, GetUvIndex(sixDirections[i]));

            }
        }

        private bool IsLeavesTransparent(ChunkData chunkData, int x, int y, int z)
        {
            for (int i = 0; i < FACES_COUNT; i++)
            {
                var adjacentBlock = Chunk.GetBlock(chunkData,
                    x + sixDirectionVectors[i].x,
                    y + sixDirectionVectors[i].y,
                    z + sixDirectionVectors[i].z);
                if (adjacentBlock == BlockType.Air || adjacentBlock == BlockType.Glass)
                    return true;
            }
            return false;
        }

    }
}
