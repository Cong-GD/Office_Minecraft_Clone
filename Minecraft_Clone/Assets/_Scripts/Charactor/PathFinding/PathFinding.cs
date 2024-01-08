using CongTDev.AStarPathFinding;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace Minecraft.AI
{
    public class PathFinding : MonoBehaviour
    {
        [SerializeField, Min(10)]
        private int maxSearchProcess = 1000;

        [SerializeField]
        [Tooltip("Highly recommended to use SquaredEuclidean for performance reason")]
        private DistanceType distanceType = DistanceType.SquaredEuclidean;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ThreadSafePool<VoxelNode>.Capacity = 50_000;
            ThreadSafePool<VoxelSearchContext>.Capacity = 20;
        }

        public VoxelSearchContext.CancelToken FindPathAsync(ISearcher searcher, Vector3 start, Vector3 end, bool flattenY)
        {
            if(searcher.IsUnityNull())
                throw new System.ArgumentNullException("Searcher is null");

            VoxelSearchContext context = ThreadSafePool<VoxelSearchContext>.Get();
            context.SetStartPosition(start);
            context.SetEndPosition(end);
            context.DistanceType = distanceType;
            context.Searcher = searcher;
            context.FlattenY = flattenY;
            FindPathAsyncInternal(context, searcher);
            return context.GetToken();
        }

        private async void FindPathAsyncInternal(VoxelSearchContext context ,ISearcher searcher)
        {
            try
            {
                await Task.Run(() =>
                {
                    AStarPathFinding.FindPath(context, maxSearchProcess);
                });
            }
            catch (System.Exception e)
            {
                context.Error = true;
                Debug.LogException(e);
            }
            finally
            {
                if (!context.Cancelled)
                {
                    searcher.OnPathFound(context.GetResult());
                }
                context.CleanUp();
                ThreadSafePool<VoxelSearchContext>.Release(context);
            }
        }
    }
}
