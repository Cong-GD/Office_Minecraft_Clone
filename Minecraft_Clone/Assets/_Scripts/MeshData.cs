using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public ArrayBuffer<Vector3> vertices = new(5000);
    public ArrayBuffer<Vector3> normals = new(5000);
    public ArrayBuffer<Vector2> uvs = new(5000);
    public ArrayBuffer<int> triangles = new(5000);
    public ArrayBuffer<int> colliderTriangles = new(5000);
    public ArrayBuffer<int> transparentTriangles = new(5000);

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
