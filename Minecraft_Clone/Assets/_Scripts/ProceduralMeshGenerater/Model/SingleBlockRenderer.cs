using NaughtyAttributes;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Minecraft.Entity.Model
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent (typeof(MeshFilter))]
    public class SingleBlockRenderer : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private SingleBlockModelData modelData;

        [SerializeField]
        private Mesh _mesh;

        public Material Material
        {
            get => meshRenderer.material;
            set => meshRenderer.material = value;
        }

        private void Reset()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
        }

        [Button]
        private void Clear()
        {
            DestroyImmediate(_mesh);
            _mesh = null;
        }

        [Button]
        private void Render()
        {
            Clear();
            _mesh = CreateMesh(modelData);
            meshFilter.sharedMesh = _mesh;
        }
         

        private static Mesh CreateMesh(SingleBlockModelData entityModelData)
        {
            Mesh mesh = new Mesh();
            var sixDirections = DirectionExtensions.SixDirections;
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangle = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                var uvsValue = entityModelData.GetUvsPosition(sixDirections[i]);
                MeshDrawerHelper.AddQuadVertices(vertices, sixDirections[i], entityModelData.offset, entityModelData.size);
                MeshDrawerHelper.AddQuadTriangle(triangle, vertices.Count);
                MeshDrawerHelper.AddQuadUvs(uvs, uvsValue.position, uvsValue.size, entityModelData.textureSize);

                if (!entityModelData.drawCloth)
                    continue;

                float3 clothOffset = entityModelData.offset * entityModelData.clothScale;
                float3 clothSize = entityModelData.size * entityModelData.clothScale;
                uvsValue.position += entityModelData.clothUvsOffset;
                MeshDrawerHelper.AddQuadVertices(vertices, sixDirections[i], clothOffset, clothSize);
                MeshDrawerHelper.AddQuadTriangle(triangle, vertices.Count);
                MeshDrawerHelper.AddQuadUvs(uvs, uvsValue.position, uvsValue.size, entityModelData.textureSize);
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangle, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}


