using UnityEngine;

namespace Minecraft.ProceduralTerrain
{
    public abstract class BlockLayerHandler : MonoBehaviour
    {
        [SerializeField] 
        private BlockLayerHandler Next;

        private string layerName;

        private bool hasNext;

        private void Awake()
        {
            layerName = name;
            hasNext = Next != null;
        }

        public bool Handle(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
        {
            try
            {
                if (TryHandling(chunkData, x, y, z, surfaceHeightNoise))
                    return true;
            }
            catch(System.Exception e)
            {
                throw new System.Exception($"Error when handler {layerName}", e);
            }
            if (hasNext)
                return Next.Handle(chunkData, x, y, z, surfaceHeightNoise);

            return false;
        }

        protected abstract bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise);
    }
}