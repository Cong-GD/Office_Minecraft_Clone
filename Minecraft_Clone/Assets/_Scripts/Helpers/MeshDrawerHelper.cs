using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class MeshDrawerHelper
{
    public const int ATLAS_SIZE = 16;
    public const float NOMALIZED_UV_VALUE = 1f / ATLAS_SIZE;
    public const int VOXEL_FACES_COUNT = 6;

    public static Texture2D GetPackedAtlas()
    {
        return Resources.Load<Texture2D>("Packed_Atlas");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddQuadVertices(ICollection<Vector3> vertices,Direction direction, int x, int y, int z)
    {
        const float blockSize = 1.002f;
        switch (direction)
        {
            case Direction.Forward:
                vertices.Add(new Vector3(x + blockSize, y, z + blockSize));
                vertices.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                vertices.Add(new Vector3(x, y + blockSize, z + blockSize));
                vertices.Add(new Vector3(x, y, z + blockSize));
                break;                        
            case Direction.Backward:
                vertices.Add(new Vector3(x, y , z ));
                vertices.Add(new Vector3(x, y + blockSize, z));
                vertices.Add(new Vector3(x + blockSize, y + blockSize, z ));
                vertices.Add(new Vector3(x + blockSize, y, z));
                break;
            case Direction.Up:
                vertices.Add(new Vector3(x, y + blockSize, z));
                vertices.Add(new Vector3(x, y + blockSize, z + blockSize));
                vertices.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                vertices.Add(new Vector3(x + blockSize, y + blockSize, z));
                break;
            case Direction.Down:
                vertices.Add(new Vector3(x, y, z + blockSize));
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x + blockSize, y, z));
                vertices.Add(new Vector3(x + blockSize, y, z + blockSize));
                break;
            case Direction.Right:
                vertices.Add(new Vector3(x + blockSize, y, z));
                vertices.Add(new Vector3(x + blockSize, y + blockSize, z));
                vertices.Add(new Vector3(x + blockSize, y + blockSize, z + blockSize));
                vertices.Add(new Vector3(x + blockSize, y, z + blockSize));
                break;                       
            case Direction.Left:
                vertices.Add(new Vector3(x, y, z + blockSize));
                vertices.Add(new Vector3(x, y + blockSize, z + blockSize));
                vertices.Add(new Vector3(x, y + blockSize, z));
                vertices.Add(new Vector3(x, y, z));
                break;                        
        }
    }

    public static void AddQuadVertices(ICollection<Vector3> vertices, Direction direction, float x, float y, float z, float size)
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

    public static void AddQuadVertices(ICollection<Vector3> vertices, Direction direction, Vector3 position, Vector3 size)
    {
        position.Parse(out float x, out float y, out float z);
        switch (direction)
        {
            case Direction.Forward:
                vertices.Add(new Vector3(x + size.x, y, z + size.z));
                vertices.Add(new Vector3(x + size.x, y + size.y, z + size.z));
                vertices.Add(new Vector3(x, y + size.y, z + size.z));
                vertices.Add(new Vector3(x, y, z + size.z));
                break;
            case Direction.Backward:
                vertices.Add(new Vector3(x, y, z));
                vertices.Add(new Vector3(x, y + size.y, z));
                vertices.Add(new Vector3(x + size.x, y + size.y, z));
                vertices.Add(new Vector3(x + size.x, y, z));
                break;
            case Direction.Up:
                vertices.Add(new Vector3(x + size.x, y + size.y, z + size.z));
                vertices.Add(new Vector3(x +size.x, y + size.y, z));
                vertices.Add(new Vector3(x, y + size.y, z));
                vertices.Add(new Vector3(x, y+ size.y, z + size.z));
                break;
            case Direction.Down:
                vertices.Add(new Vector3(x , y, z + size.z));
                vertices.Add(new Vector3(x , y, z));
                vertices.Add(new Vector3(x + size.x, y, z));
                vertices.Add(new Vector3(x + size.x, y, z + size.z));
                break;
            case Direction.Right:
                vertices.Add(new Vector3(x + size.x, y, z));
                vertices.Add(new Vector3(x + size.x, y + size.y, z));
                vertices.Add(new Vector3(x + size.x, y + size.y, z + size.z));
                vertices.Add(new Vector3(x + size.x, y, z + size.z));
                break;
            case Direction.Left:
                vertices.Add(new Vector3(x, y, z + size.z));
                vertices.Add(new Vector3(x, y + size.y, z + size.z));
                vertices.Add(new Vector3(x, y + size.y, z));
                vertices.Add(new Vector3(x, y, z));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddQuadUvs(ICollection<Vector2> uvs, int uvIndex)
    {
        float yPos = uvIndex / ATLAS_SIZE * NOMALIZED_UV_VALUE;
        float xPos = uvIndex % ATLAS_SIZE * NOMALIZED_UV_VALUE;

        uvs.Add(new Vector2(xPos, yPos));
        uvs.Add(new Vector2(xPos, yPos + NOMALIZED_UV_VALUE));
        uvs.Add(new Vector2(xPos + NOMALIZED_UV_VALUE, yPos + NOMALIZED_UV_VALUE));
        uvs.Add(new Vector2(xPos + NOMALIZED_UV_VALUE, yPos));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddQuadUvs(ICollection<Vector2> uvs, int2 positionInPixel, int2 sizeInPixel, int2 textureSizeInPixel)
    {
        positionInPixel.y = textureSizeInPixel.y - positionInPixel.y;
        float2 normalizedValue = 1f / (float2)textureSizeInPixel;
        float2 nomalizedSize = sizeInPixel * normalizedValue;
        float2 nomalizedPosition = positionInPixel * normalizedValue;
        uvs.Add(nomalizedPosition);
        uvs.Add(new Vector2(nomalizedPosition.x, nomalizedPosition.y + nomalizedSize.y));
        uvs.Add(new Vector2(nomalizedPosition.x + nomalizedSize.x, nomalizedPosition.y + nomalizedSize.y));
        uvs.Add(new Vector2(nomalizedPosition.x + nomalizedSize.x, nomalizedPosition.y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddQuadTriangle(ICollection<int> triangles, int verticesCount)
    {
        triangles.Add(verticesCount - 4);
        triangles.Add(verticesCount - 3);
        triangles.Add(verticesCount - 2);

        triangles.Add(verticesCount - 4);
        triangles.Add(verticesCount - 2);
        triangles.Add(verticesCount - 1);
    }
}
