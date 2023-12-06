using System;
using System.Linq;
using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [Serializable]
    public class TextureBaseMeshGenerator
    {
        private class MeshGenerator
        {
            private readonly Vector3Int[] vectors = VectorExtensions.SixDirectionsVector3Int.ToArray();
            private readonly Direction[] directions = DirectionExtensions.SixDirections.ToArray();

            private int height;
            private int width;
            private float nomalizedX;
            private float nomalizedY;
            private Color[] colors;
            private float size;

            private readonly MyList<Vector3> vertices = new (100);
            private readonly MyList<int> triangles = new (100);
            private readonly MyList<Vector2> uvs = new (100);

            public void GetMesh(Mesh mesh, Texture2D texture2D, float size)
            {
                this.size = size;
                SetTexture(texture2D);
                Generate();
                UpdateMesh(mesh);
            }

            private void SetTexture(Texture2D texture2D)
            {
                height = texture2D.height;
                width = texture2D.width;
                nomalizedX = 1f / width;
                nomalizedY = 1f / height;
                colors = texture2D.GetPixels();
                vertices.Clear();
                triangles.Clear();
                uvs.Clear();
            }

            private void Generate()
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (Istransparent(x, y))
                            continue;

                        DrawPixel(x, y);
                    }
                }
            }

            private void UpdateMesh(Mesh mesh)
            {
                mesh.Clear();
                mesh.SetVertices(vertices.Items, 0, vertices.Count);
                mesh.SetTriangles(triangles.Items, 0, triangles.Count, 0);
                mesh.SetUVs(0, uvs.Items, 0, uvs.Count);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
            }

            private void DrawPixel(int x, int y)
            {
                for (int i = 0; i < 6; i++)
                {
                    var nearX = x + vectors[i].x;
                    var nearY = y + vectors[i].y;
                    if (vectors[i].z == 0 && !Istransparent(nearX, nearY))
                        continue;

                    MeshDrawerHelper.AddQuadVertices(vertices, directions[i], x * size, y * size, 0, size);
                    MeshDrawerHelper.AddQuadTriangle(triangles, vertices.Count);
                    AddQuadUvs(x, y);
                }
            } 

            private void AddQuadUvs(int x, int y)
            {
                float xPos = x * nomalizedX;
                float yPos = y * nomalizedY;
                uvs.Add(new Vector2(xPos, yPos));
                uvs.Add(new Vector2(xPos, yPos + nomalizedY));
                uvs.Add(new Vector2(xPos + nomalizedY, yPos + nomalizedY));
                uvs.Add(new Vector2(xPos + nomalizedY, yPos));
            }

            private bool Istransparent(int x, int y)
            {
                int index = GetIndex(x, y);
                if(index < 0 || index >= colors.Length)
                    return true;

                return colors[index].a < 0.3f;
            }

            private int GetIndex(int x, int y) => y * height + x;

        }

        public Texture2D texture2D;

        public Material materialPrefab;

        public float size = 0.01f;

        public Material GetMaterial()
        {
            var material = new Material(materialPrefab);
            ObjectManager.AddToManagingList(material);
            material.mainTexture = texture2D;
            return material;
        }

        public Mesh GenerateMesh()
        {
            Mesh mesh = new Mesh();
            ObjectManager.AddToManagingList(mesh);
            var meshGenerator = ClassCache<MeshGenerator>.Get();
            meshGenerator.GetMesh(mesh, texture2D, size);
            ClassCache<MeshGenerator>.Release(meshGenerator);
            return mesh;
        }      
    }
}