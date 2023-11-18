using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    public class MinecraftObjectRenderer : MonoBehaviour
    {
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        private ObjectMeshData objectMeshData;

        public void RenderObject(ObjectMeshData objectMeshData, ItemTransformState state)
        {
            Clear();
            this.objectMeshData = objectMeshData;

            if (objectMeshData.mesh.triangles.Length == 0)
            {
                return;
            }

            meshFilter.sharedMesh = objectMeshData.mesh;
            meshRenderer.sharedMaterial = objectMeshData.material;
            objectMeshData.itemTransforms?.GetRelativeTransfrom(state).Apply(transform);
        }

        public void SetTransformState(ItemTransformState state)
        {
            objectMeshData.itemTransforms?.GetRelativeTransfrom(state).Apply(transform);
        }

        public void Clear()
        {
            objectMeshData = default;
            meshFilter.sharedMesh = null;
            meshRenderer.sharedMaterial = null;
        }
    }
}