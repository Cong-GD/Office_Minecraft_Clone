using UnityEngine;

namespace My.GenerateMeshMethod
{
    [CreateAssetMenu(menuName = "Minecraft/Generate Mesh Data Method/Air")]
    public class Air_SO : MeshDataGenerator_SO
    {
        public override void GetMeshData(MeshData meshData, ChunkData chunkData, Vector3Int localPos)
        {
            return;
        }
    }
}
