using UnityEngine;

public class DomainWarping : MonoBehaviour
{
    [SerializeField] private NoiseSettings noiseDomainX, noiseDomainY;
    [SerializeField] private int amplitudeX = 20, amplitudeY = 20;

    public float GenerateDomainNoise(float x, float z, NoiseSettings defaultNoiseSettings)
    {
        GenerateDomainOffset(ref x, ref z);
        return Noise.OctavePerlin(x , z, defaultNoiseSettings);
    }

    public void GenerateDomainOffset(ref float x,ref float z)
    {
        x += Noise.OctavePerlin(x, z, noiseDomainX) * amplitudeX;
        z += Noise.OctavePerlin(x, z, noiseDomainY) * amplitudeY;
    }

    public Vector2Int GenerateDomainOffsetInt(float x, float z)
    {
        GenerateDomainOffset(ref x, ref z);
        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
    }
}
