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
    private ItemRelaviteTransforms statedTransforms;

    [NonSerialized]
    private Mesh _cachedMesh;

    [NonSerialized]
    private Material _cachedMaterial;

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

        return new ObjectMeshData(_cachedMesh, _cachedMaterial, statedTransforms);
    }


    protected abstract Mesh CreateMesh();
    protected abstract Material CreateMaterial();
}
