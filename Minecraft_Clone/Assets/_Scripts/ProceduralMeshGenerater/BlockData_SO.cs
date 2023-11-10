using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/Block Data")]
public class BlockData_SO : BaseItem_SO
{

    [field: Header("Block data"), SerializeField] 
    public BlockType BlockType { get; private set; }


    [field: SerializeField] 
    public bool IsSolid { get; private set; }


    [field: SerializeField] 
    public bool IsTransparent { get; private set; }


    [field: SerializeField, Expandable] 
    public BlockMeshDataGenerator_SO MeshGenerator { get; private set; }

    protected override Mesh CreateMesh()
    {
        return MeshGenerator.CreateMesh();
    }

    protected override Material CreateMaterial()
    {
        return MeshGenerator.CreateMaterial();
    }
}
