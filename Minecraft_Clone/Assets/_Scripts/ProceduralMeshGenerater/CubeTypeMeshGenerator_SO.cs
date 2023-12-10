using CongTDev.Collection;
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

        public override Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            ObjectManager.AddToManagingList(mesh);
            MyList<Vector3> vertices = new();
            MyList<int> triangles = new();
            MyList<Vector2> uvs = new();
            for (int i = 0; i < FACES_COUNT; i++)
            {
                MeshDrawerHelper.AddQuadVertices(vertices, sixDirections[i], 0, 0, 0);
                MeshDrawerHelper.AddQuadTriangle(triangles, vertices.Count);
                MeshDrawerHelper.AddQuadUvs(uvs, GetUvIndex(sixDirections[i]));
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

        protected int GetUvIndex(Direction face, Direction blockDirection)
        {
            return blockDirection switch
            {
                Direction.Forward => face switch
                {
                    Direction.Up => up,
                    Direction.Forward => forward,
                    Direction.Backward => backward,
                    Direction.Right => right,
                    Direction.Left => left,
                    Direction.Down => down,
                    _ => 0
                },
                Direction.Backward => face switch
                {
                    Direction.Up => up,
                    Direction.Forward => backward,
                    Direction.Backward => forward,
                    Direction.Right => left,
                    Direction.Left => right,
                    Direction.Down => down,
                    _ => 0
                },
                Direction.Left => face switch
                {
                    Direction.Up => up,
                    Direction.Forward => right,
                    Direction.Backward => left,
                    Direction.Right => backward,
                    Direction.Left => forward,
                    Direction.Down => down,
                    _ => 0
                },
                Direction.Right => face switch
                {
                    Direction.Up => up,
                    Direction.Forward => left,
                    Direction.Backward => right,
                    Direction.Right => forward,
                    Direction.Left => backward,
                    Direction.Down => down,
                    _ => 0
                },
                Direction.Up => face switch
                {
                    Direction.Up => up,
                    Direction.Forward => forward,
                    Direction.Backward => backward,
                    Direction.Right => right,
                    Direction.Left => left,
                    Direction.Down => down,
                    _ => 0
                },
                Direction.Down => face switch
                {
                    Direction.Up => up,
                    Direction.Forward => forward,
                    Direction.Backward => backward,
                    Direction.Right => right,
                    Direction.Left => left,
                    Direction.Down => down,
                    _ => 0
                },
                _ => 0,
            };
        }
    }
}
