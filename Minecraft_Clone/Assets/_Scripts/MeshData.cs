using CongTDev.Collection;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public Vector3Int position;

    public MyNativeList<Vector3> vertices = new(5000);
    public MyNativeList<Vector3> normals = new(5000);
    public MyNativeList<Vector2> uvs = new(5000);
    public MyList<int> triangles = new(5000);
    public MyList<int> colliderTriangles = new(5000);
    public MyList<int> waterTriangles = new(5000);

    ~MeshData()
    {
        vertices.Dispose();
        normals.Dispose();
        uvs.Dispose();
    }

    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        colliderTriangles.Clear();
        waterTriangles.Clear();
        normals.Clear();
    }

    public bool HasDataToRender()
    {
        return vertices.Count > 0;
    }
}
