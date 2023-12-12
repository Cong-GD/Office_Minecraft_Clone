using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Water Mesh")]
    public class WaterMeshGenerator_SO : CubeTypeMeshGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int localX, int localY, int localZ)
        {
            for (int i = 0; i < FACES_COUNT; i++)
            {
                Vector3Int directionVector = sixDirectionVectors[i];
                var adjacentBlockData = Chunk.GetBlock(chunkData,
                    localX + directionVector.x,
                    localY + directionVector.y,
                    localZ + directionVector.z).Data();

                if (adjacentBlockData.BlockType == BlockType.Water)
                    continue;

                if (adjacentBlockData.BlockType != BlockType.Glass && adjacentBlockData.IsSolid)
                    continue;

                Direction direction = sixDirections[i];
                MeshDrawerHelper.AddQuadVertices(meshData.vertices, direction, localX, localY, localZ);

                for (int j = 0; j < 4; j++)
                    meshData.normals.Add(directionVector);

                MeshDrawerHelper.AddQuadTriangle(meshData.waterTriangles, meshData.vertices.Count);
                meshData.uvs.Add(new Vector2(0f, 0f));
                meshData.uvs.Add(new Vector2(0f, 1f));
                meshData.uvs.Add(new Vector2(1f, 1f));
                meshData.uvs.Add(new Vector2(1f, 0f));
            }
        }
    }
}
