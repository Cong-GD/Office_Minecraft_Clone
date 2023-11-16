using Minecraft;
using NaughtyAttributes;
using System.IO;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class NoiseTest : MonoBehaviour
{
    [Expandable]
    public NoiseGenerator_SO noiseGen;

    [Delayed]
    public int width, height;
    public MeshRenderer meshRenderer;
    public Gradient gradient;
    public Material materialPrefab;

    [SerializeField, ReadOnly]
    private Material material;

    [SerializeField, ReadOnly]
    private Texture2D texture2D;

    [SerializeField, HideInInspector]
    private Color[] colors;

    private NoiseInstance _noiseInstance;
    private NoiseInstance noiseInstance => _noiseInstance ??= noiseGen.GetNoiseInstance();

    private void OnValidate()
    {
        DestroyImmediate(material);
        DestroyImmediate(texture2D);
        material = new Material(materialPrefab);
        texture2D = new Texture2D(width, height);

        texture2D.filterMode = FilterMode.Point;
        material.mainTexture = texture2D;
        meshRenderer.material = material;
        colors = new Color[height * width];
    }

    private void Start()
    {
        if (World.Instance != null && World.Instance.isActiveAndEnabled)
        {
            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        PrintToTexture();
    }

    [Button]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void PrintToTexture()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var noiseValue = noiseInstance.GetNoise(x, y);
                colors[(y * height) + x] = gradient.Evaluate(noiseValue);
            }
        }
        texture2D.SetPixels(colors);
        texture2D.Apply();
    }


    [Button]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void WriteResult()
    {
        const string path = "Assets/_Data/Testing/PerlinTest.csv";

        float min = Mathf.Infinity;
        float max = Mathf.NegativeInfinity;

        StringBuilder sb = new StringBuilder();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = noiseInstance.GetNoise(x, y);
                if (value < min) min = value;
                if (value > max) max = value;
                sb.Append(value + ", ");
            }
            sb.Append("\n");
        }
        sb.Insert(0, $"Min, {min}\nMax, {max}\n");
        File.WriteAllText(path, sb.ToString());
    }
}

