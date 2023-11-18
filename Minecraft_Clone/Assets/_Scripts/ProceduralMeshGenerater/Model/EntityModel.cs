using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

namespace Minecraft.Entity
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class EntityModel : MonoBehaviour
    {
        public const int SIZE_IN_PIXEL = 64;
        public const float PIXEL_NORMALIZED = 1f / SIZE_IN_PIXEL;

        public Material material;
        public MeshFilter meshFilter;

        public Mesh mesh;
        
        public SingleBlockModelData headData;

        [Button]
        public void Clear()
        {
            DestroyImmediate(mesh);
            mesh = null;
        }

        [Button]
        public void Draw()
        {
            Clear();
            mesh = CreateMesh(headData);
            meshFilter.mesh = mesh;
        }

        public static Mesh CreateMesh(SingleBlockModelData entityModelData)
        {
            Mesh mesh = new Mesh();
            var sixDirections = DirectionExtensions.SixDirections;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangle = new List<int>();
            int2 textureSize = new int2(64, 64);
            for (int i = 0; i < 6; i++)
            {
                MeshDrawerHelper.AddQuadVertices(vertices, sixDirections[i], entityModelData.offSet, entityModelData.size);
                MeshDrawerHelper.AddQuadTriangle(triangle, vertices.Count);
                var uvsValue = entityModelData.GetUvsPosition(sixDirections[i]);
                MeshDrawerHelper.AddQuadUvs(uvs, uvsValue.position, uvsValue.size, textureSize);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangle, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            return mesh;
        }

    }

    [Serializable]
    public class SingleBlockModelData
    {
        public float3 size;
        public float3 offSet;

        [Header("Uvs position")]
        public UvsPositionAndSize left;
        public UvsPositionAndSize right;
        public UvsPositionAndSize up;
        public UvsPositionAndSize down;
        public UvsPositionAndSize front;
        public UvsPositionAndSize back;

        public UvsPositionAndSize GetUvsPosition(Direction direction)
        {
            return direction switch
            {
                Direction.Left => left,
                Direction.Forward => front,
                Direction.Backward => back,
                Direction.Right => right,
                Direction.Up => up,
                Direction.Down => down,
                _ => throw new NotImplementedException()
            };
        }

    }

    [Serializable]
    public struct UvsPositionAndSize
    {
        public int2 position;
        public int2 size;
    }
}


