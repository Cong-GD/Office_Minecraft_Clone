using System.Threading.Tasks;
using UnityEngine;
using ObjectPooling;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour, IPoolObject
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

    public event Action OnReturn;

    public ChunkData ChunkData { get; private set; }

    public Vector3Int ChunkCoord => ChunkData.chunkCoord;


    public void SetChunkData(ChunkData chunk)
    {
        ChunkData = chunk;
        transform.position = chunk.worldPosition;
    }

    public void UpdateMesh()
    {
        MeshData meshData = Chunk.GetMeshData(ChunkData);
        RenderMesh(meshData);
    }

    public async void UpdateMeshAsync()
    {
        MeshData meshData = await Task.Run(() => Chunk.GetMeshData(ChunkData));
        RenderMesh(meshData);
    }
    public int vertices;
    public int triangles;
    public void RenderMesh(MeshData meshData)
    {
        if (meshData.vertices.Count == 0)
        {
            ConcurrentPool.Release(meshData);
            return;
        }

        vertices = meshData.vertices.Count;
        triangles = meshData.triangles.Count;


        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.SetVertices(meshData.vertices);
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.SetTriangles(meshData.transparentTriangles, 1);
        mesh.SetUVs(0, meshData.uvs);
        mesh.RecalculateNormals();

        Mesh colliderMesh = new Mesh();
        colliderMesh.SetVertices(meshData.vertices);
        colliderMesh.SetTriangles(meshData.colliderTriangles, 0);
        colliderMesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = colliderMesh;
        ConcurrentPool.Release(meshData);
    }

    public void ReturnToPool()
    {
        OnReturn?.Invoke();
        ChunkData = null;
        meshFilter.mesh = null;
        meshCollider.sharedMesh = null;
    }
}
