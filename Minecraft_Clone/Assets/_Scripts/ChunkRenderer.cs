using ObjectPooling;
using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour, IPoolObject
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;

    public event Action OnReturn;

    public ChunkData ChunkData; //{ get; private set; }

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

    public Mesh mesh;
    public Mesh colliderMesh;

    private void Awake()
    {
        mesh = new Mesh();
        colliderMesh = new Mesh();
        meshFilter.mesh = mesh;
    }
    public void RenderMesh(MeshData meshData)
    {
        vertices = meshData.vertices.Count;
        triangles = meshData.triangles.Count;

        mesh.Clear();
        mesh.subMeshCount = 2;
        //mesh.SetVertices(meshData.vertices.Items, 0, meshData.vertices.Count);
        //mesh.SetTriangles(meshData.triangles.Items, 0, meshData.triangles.Count, 0);
        //mesh.SetTriangles(meshData.transparentTriangles.Items, 0, meshData.transparentTriangles.Count, 1);
        //mesh.SetUVs(0, meshData.uvs.Items, 0, meshData.uvs.Count);
        //mesh.SetNormals(meshData.normals.Items, 0, meshData.normals.Count);

        mesh.SetVertices(meshData.vertices.AsNativeArray());
        mesh.SetTriangles(meshData.triangles.Items, 0, meshData.triangles.Count, 0);
        mesh.SetTriangles(meshData.transparentTriangles.Items, 0, meshData.transparentTriangles.Count, 1);
        mesh.SetUVs(0, meshData.uvs.AsNativeArray());
        mesh.SetNormals(meshData.normals.AsNativeArray());

        meshCollider.sharedMesh = null;
        if (meshData.colliderTriangles.Count == 0)
            return;

        colliderMesh.Clear();
        //colliderMesh.SetVertices(meshData.vertices.Items, 0, meshData.vertices.Count);
        colliderMesh.SetVertices(meshData.vertices.AsNativeArray());
        colliderMesh.SetTriangles(meshData.colliderTriangles.Items, 0, meshData.colliderTriangles.Count, 0);
        meshCollider.sharedMesh = colliderMesh;
    }

    private void OnDestroy()
    {
        Destroy(mesh);
        Destroy(colliderMesh);
    }

    public void ReturnToPool()
    {
        OnReturn?.Invoke();
        ChunkData = null;
        meshCollider.sharedMesh = null;
    }

#if UNITY_EDITOR
    public Color gizmosColor;
    private void OnDrawGizmosSelected()
    {
        if (ChunkData == null)
            return;
        Gizmos.color = gizmosColor;
        var centor = ChunkData.worldPosition + (Vector3)WorldSettings.ChunkSizeVector / 2;
        Gizmos.DrawCube(centor, WorldSettings.ChunkSizeVector);
    }
#endif
}
