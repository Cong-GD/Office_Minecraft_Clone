using UnityEngine;

public abstract class BlockLayerHandler : MonoBehaviour
{
    [SerializeField] private BlockLayerHandler Next;

    public bool Handle(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
    {
        if(TryHandling(chunkData, x, y, z, surfaceHeightNoise))
            return true;
        if(Next != null)
            return Next.Handle(chunkData, x, y, z, surfaceHeightNoise);

        return false;
    }

    protected abstract bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise);
}
