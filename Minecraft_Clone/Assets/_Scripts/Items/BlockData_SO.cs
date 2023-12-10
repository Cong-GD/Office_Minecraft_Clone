using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Minecraft/Item/Block Data")]
public class BlockData_SO : BaseItem_SO
{

    [field: Header("Block data"), SerializeField] 
    public BlockType BlockType { get; private set; }


    [field: SerializeField] 
    public bool IsSolid { get; private set; }


    [field: SerializeField] 
    public bool IsTransparent { get; private set; }


    [field: SerializeField]
    [field: BoxGroup("Mining")]
    public float Hardness { get; private set; } = 1f;

    [field: SerializeField]
    [field: BoxGroup("Mining")]
    public ToolType BestTool { get; private set; }

    [field: EnumFlags]
    [field: SerializeField]
    [field: BoxGroup("Mining")]
    public ToolTier HarvestableTier { get; private set; }

    [field: EnumFlags]
    [field: SerializeField]
    [field: BoxGroup("Mining")]
    public ToolType HarvestableTool { get; private set; }

    [field: SerializeField]
    [field: BoxGroup("Mining")]
    public ItemPacked HarvestResult { get; private set; }

    [field: SerializeField, Expandable] 
    public BlockMeshDataGenerator_SO MeshGenerator { get; private set; }

    [NonSerialized]
    private Mesh _cachedMesh;

    protected override Mesh CreateMesh()
    {
        return MeshGenerator.CreateMesh();
    }

    protected override Material CreateMaterial()
    {
        return MeshGenerator.CreateMaterial();
    }

    public Mesh GetMeshWithoutUvAtlas()
    {
        if(_cachedMesh == null)
        {
            _cachedMesh = MeshGenerator.CreateMeshWithoutUvAtlas();
        }
        return _cachedMesh;
    }

    public bool CanHarvestBy(ITool tool)
    {
        bool isMeetTierRequiment = (HarvestableTier & tool.ToolTier) != 0;
        bool isMeetTypeRequiment = (HarvestableTool & tool.ToolType) != 0;
        return isMeetTierRequiment && isMeetTypeRequiment;
    }

    public ItemPacked GetHarvestResult(ITool tool)
    {
        if (CanHarvestBy(tool))
            return HarvestResult;

        return ItemPacked.Empty;
    }

}
