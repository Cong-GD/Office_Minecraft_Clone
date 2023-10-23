using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class BlockHelper
{

    public static readonly int AtlasSize = 16;
    public static readonly float Nomalized_UV_Value = 1f / AtlasSize;


    private static readonly Vector2[] _nomalized_UV_Vector =
{
        new Vector2 (0, 0),
        new Vector2 (0, Nomalized_UV_Value),
        new Vector2 (Nomalized_UV_Value, Nomalized_UV_Value),
        new Vector2 (Nomalized_UV_Value, 0),
    };

    public static void AddBlockMeshData(ChunkData chunkData,MeshData meshData, Vector3Int localPos)
    {
        var blockData = BlockDataManager.GetData(chunkData.GetBlock(localPos));
        if (blockData.blockType == BlockType.Air)
            return;

        foreach (var direction in DirectionExtensions.GetDirections())
        {
            var adjacentLocalPosition = localPos + direction.GetVector();
            BlockData_SO adjacentBlockData;

            if(Chunk.IsPositionInChunk( adjacentLocalPosition))
            {
                adjacentBlockData = BlockDataManager.GetData(chunkData.GetBlock(adjacentLocalPosition));
            }
            else
            {
                adjacentBlockData = World.Instance.GetBlockData(chunkData.worldPosition + adjacentLocalPosition);
            }
            
            if(adjacentBlockData.isTransparent && blockData.blockType != adjacentBlockData.blockType)
            {
                AddGetDirectionVertices(meshData.vertices, direction, localPos);

                for (int i = 0; i < 4; i++) meshData.normals.Add(direction.GetVector());

                if (blockData.isTransparent)
                {
                    GetQuadTriangle(meshData.vertices.Count, meshData.transparentTriangles);
                }
                else
                {
                    GetQuadTriangle(meshData.vertices.Count, meshData.triangles);
                }

                //meshData.triangles.AddRange(GetQuadTriangle(meshData.vertices.Count));

                if (blockData.isSolid && !adjacentBlockData.isSolid)
                {
                    GetQuadTriangle(meshData.vertices.Count, meshData.colliderTriangles);
                }
                AddUvs(meshData, blockData.GetUvIndex(direction));
            }
        }
    }

    public static void AddGetDirectionVertices(List<Vector3> vertices,Direction direction,Vector3Int position)
    {
        position.Parse(out var x, out var y, out var z);
        switch (direction)
        {
            case Direction.Forward:
                vertices.Add(new Vector3(x + 1, y + 0, z + 1));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x + 0, y + 1, z + 1));
                vertices.Add(new Vector3(x + 0, y + 0, z + 1));
                break;                        
            case Direction.Backward:
                vertices.Add(new Vector3(x + 0, y + 0, z + 0));
                vertices.Add(new Vector3(x + 0, y + 1, z + 0));
                vertices.Add(new Vector3(x + 1, y + 1, z + 0));
                vertices.Add(new Vector3(x + 1, y + 0, z + 0));
                break;                                       
            case Direction.Right:                            
                vertices.Add(new Vector3(x + 1, y + 0, z + 0));
                vertices.Add(new Vector3(x + 1, y + 1, z + 0));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x + 1, y + 0, z + 1));
                break;                       
            case Direction.Left:
                vertices.Add(new Vector3(x + 0, y + 0, z + 1));
                vertices.Add(new Vector3(x + 0, y + 1, z + 1));
                vertices.Add(new Vector3(x + 0, y + 1, z + 0));
                vertices.Add(new Vector3(x + 0, y + 0, z + 0));
                break;                        
            case Direction.Up:
                vertices.Add(new Vector3(x + 0, y + 1, z + 0));
                vertices.Add(new Vector3(x + 0, y + 1, z + 1));
                vertices.Add(new Vector3(x + 1, y + 1, z + 1));
                vertices.Add(new Vector3(x + 1, y + 1, z + 0));
                break;               
            case Direction.Down:
                vertices.Add(new Vector3(x + 0, y + 0, z + 1));
                vertices.Add(new Vector3(x + 0, y + 0, z + 0));
                vertices.Add(new Vector3(x + 1, y + 0, z + 0));
                vertices.Add(new Vector3(x + 1, y + 0, z + 1));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddUvs(MeshData meshData, int uvIndex)
    {
        int yPos = uvIndex / AtlasSize;
        int xPos = uvIndex % AtlasSize;

        Vector2 uv1 = new Vector2(xPos * Nomalized_UV_Value, yPos * Nomalized_UV_Value);
        meshData.uvs.Add(uv1);
        meshData.uvs.Add(uv1 + _nomalized_UV_Vector[1]);
        meshData.uvs.Add(uv1 + _nomalized_UV_Vector[2]);
        meshData.uvs.Add(uv1 + _nomalized_UV_Vector[3]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetQuadTriangle(int verticesCount, List<int> triangles)
    {
        triangles.Add(verticesCount - 4);
        triangles.Add(verticesCount - 3);
        triangles.Add(verticesCount - 2);

        triangles.Add(verticesCount - 4);
        triangles.Add(verticesCount - 2);
        triangles.Add(verticesCount - 1);
    }
}
