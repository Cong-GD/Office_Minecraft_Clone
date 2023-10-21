using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        var blockData = BlockDataManager.GetData(chunkData.GetBlock(ref localPos));
        if (blockData.blockType == BlockType.Air)
            return;

        foreach (var direction in DirectionExtensions.GetDirections())
        {
            var adjacentLocalPosition = localPos + direction.GetVector();
            BlockData adjacentBlockData;
            if(Chunk.IsPositionInChunk( adjacentLocalPosition))
            {
                adjacentBlockData = BlockDataManager.GetData(chunkData.GetBlock(ref adjacentLocalPosition));
            }
            else
            {
                adjacentBlockData = World.Instance.GetBlockData(chunkData.worldPosition + adjacentLocalPosition);
            }
            
            if(adjacentBlockData.isTransparent)
            {
                meshData.vertices.AddRange(GetDirectionVertices(direction, localPos));

                if (blockData.isTransparent)
                {
                    meshData.transparentTriangles.AddRange(GetQuadTriangle(meshData.vertices.Count));
                }
                else
                {
                    meshData.triangles.AddRange(GetQuadTriangle(meshData.vertices.Count));
                }

                //meshData.triangles.AddRange(GetQuadTriangle(meshData.vertices.Count));

                if (blockData.generateCollider && !adjacentBlockData.generateCollider)
                {
                    meshData.colliderTriangles.AddRange(GetQuadTriangle(meshData.vertices.Count));
                }
                AddUvs(meshData, blockData.GetUvIndex(direction));
            }
        }
    }

    private static IEnumerable<Vector3> GetDirectionVertices(Direction direction,Vector3Int position)
    {
        position.Parse(out var x, out var y, out var z);
        switch (direction)
        {
            case Direction.Forward:
                yield return new Vector3(x + 1, y + 0, z + 1);
                yield return new Vector3(x + 1, y + 1, z + 1);
                yield return new Vector3(x + 0, y + 1, z + 1);
                yield return new Vector3(x + 0, y + 0, z + 1);
                break;                        
            case Direction.Backward:          
                yield return new Vector3(x + 0, y + 0, z + 0);
                yield return new Vector3(x + 0, y + 1, z + 0);
                yield return new Vector3(x + 1, y + 1, z + 0);
                yield return new Vector3(x + 1, y + 0, z + 0);
                break;
            case Direction.Right:
                yield return new Vector3(x + 1, y + 0, z + 0);
                yield return new Vector3(x + 1, y + 1, z + 0);
                yield return new Vector3(x + 1, y + 1, z + 1);
                yield return new Vector3(x + 1, y + 0, z + 1);
                break;                       
            case Direction.Left:             
                yield return new Vector3(x + 0, y + 0, z + 1);
                yield return new Vector3(x + 0, y + 1, z + 1);
                yield return new Vector3(x + 0, y + 1, z + 0);
                yield return new Vector3(x + 0, y + 0, z + 0);
                break;                        
            case Direction.Up:                
                yield return new Vector3(x + 0, y + 1, z + 0);
                yield return new Vector3(x + 0, y + 1, z + 1);
                yield return new Vector3(x + 1, y + 1, z + 1);
                yield return new Vector3(x + 1, y + 1, z + 0);
                break;               
            case Direction.Down:     
                yield return new Vector3(x + 0, y + 0, z + 1);
                yield return new Vector3(x + 0, y + 0, z + 0);
                yield return new Vector3(x + 1, y + 0, z + 0);
                yield return new Vector3(x + 1, y + 0, z + 1);
                break;
        }
    }

    private static void AddUvs(MeshData meshData, int uvIndex)
    {
        int yPos = uvIndex / AtlasSize;
        int xPos = uvIndex % AtlasSize;

        Vector2 uv1 = new Vector2(xPos * Nomalized_UV_Value, yPos * Nomalized_UV_Value);
        meshData.uvs.Add(uv1);
        meshData.uvs.Add(uv1 + _nomalized_UV_Vector[1]);
        meshData.uvs.Add(uv1 + _nomalized_UV_Vector[2]);
        meshData.uvs.Add(uv1 + _nomalized_UV_Vector[3]);
    }

    private static IEnumerable<int> GetQuadTriangle(int verticesCount)
    {
        yield return verticesCount - 4;
        yield return verticesCount - 3;
        yield return verticesCount - 2;

        yield return verticesCount - 4;
        yield return verticesCount - 2;
        yield return verticesCount - 1;
    }
}
