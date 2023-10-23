using ObjectPooling;
using System;
using System.Threading.Tasks;
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
        meshFilter.mesh = mesh;
    }
    public void RenderMesh(MeshData meshData)
    {
        vertices = meshData.vertices.Count;
        triangles = meshData.triangles.Count;

        mesh.Clear();
        mesh.subMeshCount = 2;
        mesh.SetVertices(meshData.vertices);
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.SetTriangles(meshData.transparentTriangles, 1);
        mesh.SetUVs(0, meshData.uvs);
        mesh.SetNormals(meshData.normals);
        //mesh.RecalculateNormals();

        Mesh colliderMesh = new Mesh();
        colliderMesh.SetVertices(meshData.vertices);
        colliderMesh.SetTriangles(meshData.colliderTriangles, 0);
        colliderMesh.RecalculateNormals();
        meshCollider.sharedMesh = colliderMesh;
    }

    //private void OnBecameVisible()
    //{
    //    meshFilter.mesh = mesh;
    //}

    //private void OnBecameInvisible()
    //{
    //    meshFilter.mesh = null;
    //}

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
        var centor = ChunkData.worldPosition + (Vector3)GameSettings.ChunkSizeVector / 2;
        Gizmos.DrawCube(centor, GameSettings.ChunkSizeVector);
    }
#endif
}
