using Minecraft.ProceduralMeshGenerate;
using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Minecraft/Item/Functionless")]
public class FunctionlessItem_SO : BaseItem_SO
{
    [SerializeField]
    private TextureBaseMeshGenerator meshGenerator;

    protected override Material CreateMaterial()
    {
        return meshGenerator.GetMaterial();
    }

    protected override Mesh CreateMesh()
    {
        return meshGenerator.GenerateMesh();
    }
}
