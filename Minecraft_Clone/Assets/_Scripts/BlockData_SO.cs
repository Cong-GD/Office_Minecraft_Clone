using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Block Data")]
public class BlockData_SO : ScriptableObject, IItem
{



    [field: SerializeField] 
    public BlockType BlockType { get; private set; }


    [field: SerializeField] 
    public bool IsSolid { get; private set; }


    [field: SerializeField] 
    public bool IsTransparent { get; private set; }


    [field: SerializeField]
    [field: ShowAssetPreview] 
    public Sprite Icon { get; private set; }


    [field: SerializeField] 
    public int MaxStack { get; private set; } = 64;


    [field: SerializeField, Expandable] 
    public BlockMeshDataGenerator_SO MeshGenerator { get; private set; }

    public string Name => name;

    public ObjectMeshData ObjectMeshData => MeshGenerator.GetObjectMeshData();

    public bool Equals(IItem other)
    {
        if(other is not BlockData_SO blockData)
            return false;
        return this == blockData;
    }
}