using UnityEngine;

namespace Minecraft.ProceduralMeshGenerate
{
    [CreateAssetMenu(menuName = "Minecraft/ProceduralMeshGenerator/Air")]
    public class AirMeshGenerator_SO : BlockMeshDataGenerator_SO
    {
        public override void GetMeshData(ChunkData chunkData, MeshData meshData, int x, int y, int z)
        {
        }

        public override Mesh CreateMesh()
        {
            return null;
        }
    }
}
