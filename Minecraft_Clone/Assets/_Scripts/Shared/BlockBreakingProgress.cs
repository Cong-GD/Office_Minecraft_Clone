using Unity.Mathematics;
using UnityEngine;

public class BlockBreakingProgress : ProgressDisplayer
{
    [SerializeField]
    private Texture2D[] breakingTextures;

    [SerializeField]
    private Renderer breakingRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private float offset;

    private Material _material;

    private Vector3 Offset => new Vector3(offset, offset, offset);

    private void Awake()
    {
        _material = breakingRenderer.material;
    }

    private void OnDestroy()
    {
        DestroyImmediate(_material);
    }

    public void SetMeshAndPosition(Mesh mesh, Vector3 position)
    {
        transform.position = position + Offset;
        meshFilter.mesh = mesh;
        _material.mainTexture = breakingTextures[0];
    }

    public void OnValueChanged(float value)
    {
        _material.mainTexture = breakingTextures[GetTextureIndex(value)];
    }

    private int GetTextureIndex(float value)
    {
        int index = (int)(value * (breakingTextures.Length - 1));
        return index;
    }

}