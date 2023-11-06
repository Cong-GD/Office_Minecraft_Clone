using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    public abstract class CubeTypeMeshGenerator_SO : BlockMeshDataGenerator_SO
    {
        public const int FACES_COUNT = 6;

        [Header("Uv Index")]
        [SerializeField] private int up;
        [SerializeField] private int down;
        [SerializeField] private int left;
        [SerializeField] private int right;
        [SerializeField] private int forward;
        [SerializeField] private int backward;

        protected static Vector3Int[] sixDirectionVectors = VectorExtensions.SixDirectionsVector3Int.ToArray();
        protected static Direction[] sixDirections = DirectionExtensions.SixDirections.ToArray();

        protected override Mesh GenerateObjectMesh()
        {
            Mesh mesh = new Mesh();
            ArrayBuffer<Vector3> vertices = new();
            ArrayBuffer<int> triangles = new();
            ArrayBuffer<Vector2> uvs = new();
            for (int i = 0; i < FACES_COUNT; i++)
            {
                VoxelHelper.AddQuadVertices(vertices, sixDirections[i], 0, 0, 0, 0.1f);
                VoxelHelper.AddQuadTriangle(triangles, vertices.Count);
                VoxelHelper.AddQuadUvs(uvs, GetUvIndex(sixDirections[i]));
            }
            mesh.SetVertices(vertices.Items, 0, vertices.Count);
            mesh.SetTriangles(triangles.Items, 0, triangles.Count, 0);
            mesh.SetUVs(0, uvs.Items, 0, uvs.Count);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        protected int GetUvIndex(Direction direction)
        {
            return direction switch
            {
                Direction.Up => up,
                Direction.Forward => forward,
                Direction.Backward => backward,
                Direction.Right => right,
                Direction.Left => left,
                Direction.Down => down,
                _ => 0
            };
        }
    }
}
