using UnityEngine;

public class StoneLayerHandler : BlockLayerHandler
{
    [Range(0, 1)]
    public float stoneThreashold = 0.5f;

    public NoiseSettings stoneNoiseSettings;
    public DomainWarping domainWrapping;

    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
    {
        if(chunkData.worldPosition.y > surfaceHeightNoise)
            return false;

        //float stoneNoise = Noise.OctavePerlin(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z, stoneNoiseSettings);
        float stoneNoise = domainWrapping.GenerateDomainNoise(chunkData.worldPosition.x + x, chunkData.worldPosition.z + z, stoneNoiseSettings);
        int endPosition = surfaceHeightNoise - chunkData.worldPosition.y;

        if(stoneNoise > stoneThreashold)
        {
            for(int i = 0; i <= endPosition; i++)
            {
                chunkData.SetBlock(x, i, z, BlockType.Stone);
            }
            return true;
        }
        return false;

    }
}
