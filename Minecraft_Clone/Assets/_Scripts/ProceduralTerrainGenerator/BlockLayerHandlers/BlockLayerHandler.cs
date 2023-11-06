using UnityEngine;

namespace Minecraft.ProceduralTerrain
{
    public abstract class BlockLayerHandler : MonoBehaviour
    {
        [SerializeField] private BlockLayerHandler Next;

        private string layerName;

        private void Awake()
        {
            layerName = name;
        }

        public bool Handle(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise)
        {

            try
            {
                if (TryHandling(chunkData, x, y, z, surfaceHeightNoise))
                    return true;
            }
            catch
            {
                throw new System.Exception($"Error when handler {layerName}");
            }
            if (Next != null)
                return Next.Handle(chunkData, x, y, z, surfaceHeightNoise);

            return false;
        }

        protected abstract bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise);
    }
}