using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices = new();
    public List<int> triangles = new();
    public List<Vector2> uvs = new();
    public List<int> colliderTriangles = new();
    public List<int> transparentTriangles = new();
    public List<Vector3> normals = new();

    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colliderTriangles.Clear();
        transparentTriangles.Clear();
        normals.Clear();
    }

    public bool HasDataToRender()
    {
        return vertices.Count > 0;
    }
}
