using NaughtyAttributes;
using ObjectPooling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    public class MinecraftObject : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private MeshCollider meshCollider;


        #region Testing
        public BlockData_SO blockData;

        [Button]
        public void RenderBlock()
        {
            RenderObject(ItemTransformState.InRightHand, blockData.ObjectMeshData);
        }
        #endregion

        public void RenderObject(ItemTransformState state, ObjectMeshData objectMeshData)
        {
            Clear();
            if (objectMeshData.mesh.triangles.Length == 0)
            {
                Debug.LogWarning("None triangles mesh!!");
                return;
            }

            if (state == ItemTransformState.FreeObject)
            {
                meshCollider.sharedMesh = objectMeshData.mesh;
            }
            meshFilter.sharedMesh = objectMeshData.mesh;
            meshRenderer.sharedMaterial = objectMeshData.material;
            objectMeshData.itemTransforms.GetRelativeTransfrom(state).Apply(transform);
        }

        public void Clear()
        {
            meshFilter.sharedMesh = null;
            meshRenderer.sharedMaterial = null;
            meshCollider.sharedMesh = null;
        }



        //[Button]
        //public void RenderTexture()
        //{

        //    ClearMesh();
        //    material = new Material(this.materialPrefab);
        //    material.mainTexture = texture;
        //    meshRenderer.sharedMaterial = material;

        //    Color[] colors = texture.GetPixels();
        //    int width = texture.width;
        //    height = texture.height;

        //    nomX = 1f / width;
        //    nomY = 1f / height;

        //    for (int x = 0; x < width; x++)
        //    {
        //        for (int y = 0; y < height; y++)
        //        {
        //            var color = colors[GetIndex(x, y, width)];
        //            if (color.a < 0.3f)
        //                continue;

        //            DrawPixels(x, y, color);
        //        }
        //    }

        //    mesh = new Mesh();
        //    mesh.SetVertices(vertices);
        //    mesh.SetTriangles(triangles, 0);
        //    //mesh.SetColors(this.colors);
        //    mesh.SetUVs(0, uvs);
        //    mesh.RecalculateNormals();
        //    meshFilter.sharedMesh = mesh;
        //}

        //private void DrawPixels(float x, float y, Color color)
        //{
        //    for (int i = 0; i < 6; i++)
        //    {
        //        AddQuadVertices(vertices, sixDirections[i], x, y, 0);
        //        for (int j = 0; j < 4; j++)
        //        {
        //            colors.Add(color);
        //        }
        //        AddUvs(x, y);
        //        AddQuadTriangle(triangles, vertices.Count);
        //    }
        //}
    }

}