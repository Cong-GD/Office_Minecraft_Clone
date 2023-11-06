using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class VoxelHelper
{

    public const int ATLAS_SIZE = 16;
    public const float NOMALIZED_UV_VALUE = 1f / ATLAS_SIZE;
    public const int VOXEL_FACES_COUNT = 6;

    public static Texture2D GetPackedAtlas()
    {
        return Resources.Load<Texture2D>("Packed_Atlas");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddQuadVertices(ArrayBuffer<Vector3> vertices,Direction direction, int x, int y, int z)
    {
        switch (direction)
        {
            case Direction.Forward:
                vertices.Add(new Vector3(x + 1, y, z + 1));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x, y + 1, z + 1));
                vertices.Add(new Vector3(x, y, z + 1));
                break;                        
            case Direction.Backward:
                vertices.Add(new Vector3(x, y , z ));
                vertices.Add(new Vector3(x, y + 1, z));
                vertices.Add(new Vector3(x + 1, y + 1, z ));
                vertices.Add(new Vector3(x + 1, y, z));
                break;
            case Direction.Up:
                vertices.Add(new Vector3(x, y + 1, z));
                vertices.Add(new Vector3(x, y + 1, z + 1));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x + 1, y + 1, z));
                break;
            case Direction.Down:
                vertices.Add(new Vector3(x, y, z + 1));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x + 1, y, z));
                vertices.Add(new Vector3(x + 1, y, z + 1));
                break;
            case Direction.Right:                            
                vertices.Add(new Vector3(x + 1, y, z));
                vertices.Add(new Vector3(x + 1, y + 1, z));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x + 1, y, z + 1));
                break;                       
            case Direction.Left:
                vertices.Add(new Vector3(x, y , z + 1));
                vertices.Add(new Vector3(x, y + 1, z + 1));
                vertices.Add(new Vector3(x, y + 1, z));
                vertices.Add(new Vector3(x, y, z));
                break;                        
        }
    }

    public static void AddQuadVertices(ArrayBuffer<Vector3> vertices, Direction direction, int x, int y, int z, float size)
    {
        switch (direction)
        {
            case Direction.Forward:
                vertices.Add(new Vector3(x + size, y, z + size));
                vertices.Add(new Vector3(x + size, y + size, z + size));
                vertices.Add(new Vector3(x, y + size, z + size));
                vertices.Add(new Vector3(x, y, z + size));
                break;
            case Direction.Backward:
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y + size, z));
                vertices.Add(new Vector3(x + size, y + size, z));
                vertices.Add(new Vector3(x + size, y, z));
                break;
            case Direction.Up:
                vertices.Add(new Vector3(x, y + size, z));
                vertices.Add(new Vector3(x, y + size, z + size));
                vertices.Add(new Vector3(x + size, y + size, z + size));
                vertices.Add(new Vector3(x + size, y + size, z));
                break;
            case Direction.Down:
                vertices.Add(new Vector3(x, y, z + size));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x + size, y, z));
                vertices.Add(new Vector3(x + size, y, z + size));
                break;
            case Direction.Right:
                vertices.Add(new Vector3(x + size, y, z));
                vertices.Add(new Vector3(x + size, y + size, z));
                vertices.Add(new Vector3(x + size, y + size, z + size));
                vertices.Add(new Vector3(x + size, y, z + size));
                break;
            case Direction.Left:
                vertices.Add(new Vector3(x, y, z + size));
                vertices.Add(new Vector3(x, y + size, z + size));
                vertices.Add(new Vector3(x, y + size, z));
                vertices.Add(new Vector3(x, y, z));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddQuadUvs(ArrayBuffer<Vector2> uvs, int uvIndex)
    {
        float yPos = uvIndex / ATLAS_SIZE * NOMALIZED_UV_VALUE;
        float xPos = uvIndex % ATLAS_SIZE * NOMALIZED_UV_VALUE;

        uvs.Add(new Vector2(xPos, yPos));
        uvs.Add(new Vector2(xPos, yPos + NOMALIZED_UV_VALUE));
        uvs.Add(new Vector2(xPos + NOMALIZED_UV_VALUE, yPos + NOMALIZED_UV_VALUE));
        uvs.Add(new Vector2(xPos + NOMALIZED_UV_VALUE, yPos));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddQuadTriangle(ArrayBuffer<int> triangles, int verticesCount)
    {
        triangles.Add(verticesCount - 4);
        triangles.Add(verticesCount - 3);
        triangles.Add(verticesCount - 2);

        triangles.Add(verticesCount - 4);
        triangles.Add(verticesCount - 2);
        triangles.Add(verticesCount - 1);
    }
}
