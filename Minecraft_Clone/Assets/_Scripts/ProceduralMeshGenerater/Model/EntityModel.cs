using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Minecraft.Entity.Model
{
    public class EntityModel : MonoBehaviour
    {
        public Material material;

        public MeshPosition[] bodyParts;

        public Mesh[] createdMeshs;

        [Button]
        public void Clear()
        {
            foreach (var mesh in createdMeshs.AsSpan())
            {
                DestroyImmediate(mesh);
            }
            createdMeshs = null;

            
        }

        [Button]
        public void Draw()
        {
            Clear();
            createdMeshs = new Mesh[bodyParts.Length];
            for (int i = 0; i < bodyParts.Length; i++)
            {
                createdMeshs[i] = CreateMesh(bodyParts[i].data);
                var partTransform = bodyParts[i].transform;
                partTransform.GetOrAddComponent<MeshRenderer>().sharedMaterial = material;
                partTransform.GetOrAddComponent<MeshFilter>().sharedMesh = createdMeshs[i];
            }
        }

        private static Mesh CreateMesh(SingleBlockModelData entityModelData)
        {
            Mesh mesh = new Mesh();
            var sixDirections = DirectionExtensions.SixDirections;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangle = new List<int>();
            int2 textureSize = new int2(64);

            for (int i = 0; i < 6; i++)
            {
                var uvsValue = entityModelData.GetUvsPosition(sixDirections[i]);
                MeshDrawerHelper.AddQuadVertices(vertices, sixDirections[i], entityModelData.offSet, entityModelData.size);
                MeshDrawerHelper.AddQuadTriangle(triangle, vertices.Count);
                MeshDrawerHelper.AddQuadUvs(uvs, uvsValue.position, uvsValue.size, textureSize);

                if (!entityModelData.drawCloth)
                    continue;

                float3 clothOffset = entityModelData.offSet * entityModelData.clothScale;
                float3 clothSize = entityModelData.size * entityModelData.clothScale;
                uvsValue.position += entityModelData.clothUvsOffset;
                MeshDrawerHelper.AddQuadVertices(vertices, sixDirections[i], clothOffset, clothSize);
                MeshDrawerHelper.AddQuadTriangle(triangle, vertices.Count);
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

        public bool drawCloth;
        public int2 clothUvsOffset;
        public float clothScale = 1.1f;
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

    [Serializable]
    public class MeshPosition
    {
        public Transform transform;
        public SingleBlockModelData data;
    }
}


