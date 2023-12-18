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
        private int maxSearchrPocess = 10_000;

        [SerializeField]
        [Tooltip("Highly recommended to use SquaredEuclidean for performance reason")]
        private DistanceType distanceType = DistanceType.SquaredEuclidean;

        private void Awake()
        {
            ThreadSafePool<VoxelNode>.Capacity = 1_000_000;
        }

        public void FindPath(ISearcher searcher, Vector3 start, Vector3 end)
        {
            if (searcher.IsUnityNull())
                throw new System.ArgumentNullException("Searcher is null");

            VoxelSearchContext context = ThreadSafePool<VoxelSearchContext>.Get(); 
            try
            {
                context.SetStartPosition(start);
                context.SetEndPosition(end);
                context.DistanceType = distanceType;
                context.Searcher = searcher;
                AStarPathFinding.FindPath(context, maxSearchrPocess);
            }
            catch(System.Exception e)
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

        public VoxelSearchContext.Token FindPathAsync(ISearcher searcher, Vector3 start, Vector3 end)
        {
            if(searcher.IsUnityNull())
                throw new System.ArgumentNullException("Searcher is null");

            VoxelSearchContext context = ThreadSafePool<VoxelSearchContext>.Get();
            context.SetStartPosition(start);
            context.SetEndPosition(end);
            context.DistanceType = distanceType;
            context.Searcher = searcher;
            FindPathAsyncInternal(context, searcher);
            return context.GetToken();
        }

        private async void FindPathAsyncInternal(VoxelSearchContext context ,ISearcher searcher)
        {
            try
            {
                await Task.Run(() =>
                {
                    AStarPathFinding.FindPath(context, maxSearchrPocess);
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
