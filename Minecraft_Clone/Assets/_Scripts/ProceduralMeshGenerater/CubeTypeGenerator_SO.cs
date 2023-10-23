using System.Collections.Generic;
using UnityEngine;
using static GameSettings;

namespace My.GenerateMeshMethod
{
    [CreateAssetMenu(menuName = "Minecraft/Generate Mesh Data Method/Cube")]
    public class CubeTypeGenerator_SO : MeshDataGenerator_SO
    {
        private static readonly Vector2[] _nomalized_UV_Vector =
        {
        new Vector2 (0, 0),
        new Vector2 (0, Nomalized_UV_Value),
        new Vector2 (Nomalized_UV_Value, Nomalized_UV_Value),
        new Vector2 (Nomalized_UV_Value, 0),
        };


        public override void GetMeshData(MeshData meshData, ChunkData chunkData, Vector3Int localPos)
        {
            //var blockData = BlockDataManager.GetData(chunkData.GetBlock(localPos));

            //foreach (var direction in DirectionExtensions.GetDirections())
            //{
            //    var adjacentLocalPosition = localPos + direction.GetVector();
            //    BlockData_SO adjacentBlockData;
            //    if (Chunk.IsPositionInChunk(adjacentLocalPosition))
            //    {
            //        adjacentBlockData = BlockDataManager.GetData(chunkData.GetBlock(adjacentLocalPosition));
            //    }
            //    else
            //    {
            //        adjacentBlockData = World.Instance.GetBlockData(chunkData.worldPosition + adjacentLocalPosition);
            //    }

            //    if (adjacentBlockData.isTransparent)
            //    {
            //        meshData.vertices.AddRange(BlockHelper.GetDirectionVertices(direction, localPos));

            //        for (int i = 0; i < 4; i++) meshData.normals.Add(direction.GetVector());

            //        if (blockData.isTransparent)
            //        {
            //            meshData.transparentTriangles.AddRange(BlockHelper.GetQuadTriangle(meshData.vertices.Count));
            //        }
            //        else
            //        {
            //            meshData.triangles.AddRange(BlockHelper.GetQuadTriangle(meshData.vertices.Count));
            //        }

            //        //meshData.triangles.AddRange(GetQuadTriangle(meshData.vertices.Count));

            //        if (blockData.isSolid && !adjacentBlockData.isSolid)
            //        {
            //            meshData.colliderTriangles.AddRange(BlockHelper.GetQuadTriangle(meshData.vertices.Count));
            //        }
            //        BlockHelper.AddUvs(meshData, blockData.GetUvIndex(direction));
            //    }
            //}
        }


        public static IEnumerable<Vector3> GetDirectionVertices(Direction direction, Vector3Int position)
        {
            position.Parse(out var x, out var y, out var z);
            switch (direction)
            {
                case Direction.Forward:
                    yield return new Vector3(x + 1, y + 0, z + 1);
                    yield return new Vector3(x + 1, y + 1, z + 1);
                    yield return new Vector3(x + 0, y + 1, z + 1);
                    yield return new Vector3(x + 0, y + 0, z + 1);
                    break;
                case Direction.Backward:
                    yield return new Vector3(x + 0, y + 0, z + 0);
                    yield return new Vector3(x + 0, y + 1, z + 0);
                    yield return new Vector3(x + 1, y + 1, z + 0);
                    yield return new Vector3(x + 1, y + 0, z + 0);
                    break;
                case Direction.Right:
                    yield return new Vector3(x + 1, y + 0, z + 0);
                    yield return new Vector3(x + 1, y + 1, z + 0);
                    yield return new Vector3(x + 1, y + 1, z + 1);
                    yield return new Vector3(x + 1, y + 0, z + 1);
                    break;
                case Direction.Left:
                    yield return new Vector3(x + 0, y + 0, z + 1);
                    yield return new Vector3(x + 0, y + 1, z + 1);
                    yield return new Vector3(x + 0, y + 1, z + 0);
                    yield return new Vector3(x + 0, y + 0, z + 0);
                    break;
                case Direction.Up:
                    yield return new Vector3(x + 0, y + 1, z + 0);
                    yield return new Vector3(x + 0, y + 1, z + 1);
                    yield return new Vector3(x + 1, y + 1, z + 1);
                    yield return new Vector3(x + 1, y + 1, z + 0);
                    break;
                case Direction.Down:
                    yield return new Vector3(x + 0, y + 0, z + 1);
                    yield return new Vector3(x + 0, y + 0, z + 0);
                    yield return new Vector3(x + 1, y + 0, z + 0);
                    yield return new Vector3(x + 1, y + 0, z + 1);
                    break;
            }
        }

        public static void AddUvs(MeshData meshData, int uvIndex)
        {
            Vector2 uv1 = GetUVPosition(uvIndex);
            meshData.uvs.Add(uv1);
            meshData.uvs.Add(uv1 + _nomalized_UV_Vector[1]);
            meshData.uvs.Add(uv1 + _nomalized_UV_Vector[2]);
            meshData.uvs.Add(uv1 + _nomalized_UV_Vector[3]);
        }

        public static IEnumerable<int> GetQuadTriangle(int verticesCount)
        {
            yield return verticesCount - 4;
            yield return verticesCount - 3;
            yield return verticesCount - 2;

            yield return verticesCount - 4;
            yield return verticesCount - 2;
            yield return verticesCount - 1;
        }
    }

    public class SolidBlockMeshGenerator_SO : MeshDataGenerator_SO
    {
        [Header("Uv Index")]
        [SerializeField] private int up;
        [SerializeField] private int down;
        [SerializeField] private int left;
        [SerializeField] private int right;
        [SerializeField] private int forward;
        [SerializeField] private int backward;

        public override void GetMeshData(MeshData meshData, ChunkData chunkData, Vector3Int localPos)
        {
            //foreach (var direction in DirectionExtensions.GetDirections())
            //{
            //    var adjacentLocalPosition = localPos + direction.GetVector();
            //    BlockData_SO adjacentBlockData;
            //    if (Chunk.IsPositionInChunk(adjacentLocalPosition))
            //    {
            //        adjacentBlockData = BlockDataManager.GetData(chunkData.GetBlock(adjacentLocalPosition));
            //    }
            //    else
            //    {
            //        adjacentBlockData = World.Instance.GetBlockData(chunkData.worldPosition + adjacentLocalPosition);
            //    }

            //    if (adjacentBlockData.isTransparent)
            //    {
            //        meshData.vertices.AddRange(BlockHelper.AddGetDirectionVertices(direction, localPos));

            //        for (int i = 0; i < 4; i++) meshData.normals.Add(direction.GetVector());

            //        meshData.triangles.AddRange(BlockHelper.GetQuadTriangle(meshData.vertices.Count));

            //        if (!adjacentBlockData.isSolid)
            //        {
            //            meshData.colliderTriangles.AddRange(BlockHelper.GetQuadTriangle(meshData.vertices.Count));
            //        }
            //        //BlockHelper.AddUvs(meshData, blockData.GetUvIndex(direction));
            //    }
            //}
        }
    }
}
