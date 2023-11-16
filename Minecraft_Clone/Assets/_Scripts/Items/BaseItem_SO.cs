using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
using System;
using UnityEngine;

public abstract class BaseItem_SO : ScriptableObject
{
    [field: Header("Base Info")]
    [field: SerializeField]
    [field: ShowAssetPreview]
    public Sprite Icon { get; private set; }


    [field: SerializeField, Min(1)]
    public int MaxStack { get; private set; } = 64;


    [SerializeField]
    private ItemRelaviteTransforms relativeTransforms;


    [NonSerialized]
    private Mesh _cachedMesh;

    [NonSerialized]
    private Material _cachedMaterial;


    [field: Tooltip("Is this item can be used as burn fuel")]
    [field: SerializeField]
    [field: BoxGroup("Smelt And Burn")]
    public bool CanBurn { get; private set; }

    [field: SerializeField, Min(0f)]
    [field: BoxGroup("Smelt And Burn"), ShowIf("CanBurn")]
    public float BurnDuration { get; private set; } 



    [field: Tooltip("Is this item can be smelted into something")]
    [field: SerializeField]
    [field: BoxGroup("Smelt And Burn")]
    public bool CanSmelt { get; private set; }


    [field: SerializeField, Min(0f)]
    [field: BoxGroup("Smelt And Burn"), ShowIf("CanSmelt")]
    public float SmeltDuration { get; private set; }


    [field: SerializeField]
    [field: BoxGroup("Smelt And Burn"), ShowIf("CanSmelt")]
    public ItemPacked SmeltResult { get; private set; }

    public ObjectMeshData GetObjectMeshData()
    {
        if (_cachedMesh == null)
        {
            _cachedMesh = CreateMesh();
        }

        if (_cachedMaterial == null)
        {
            _cachedMaterial = CreateMaterial();
        }

        return new ObjectMeshData(_cachedMesh, _cachedMaterial, relativeTransforms);
    }


    protected abstract Mesh CreateMesh();
    protected abstract Material CreateMaterial();
}
