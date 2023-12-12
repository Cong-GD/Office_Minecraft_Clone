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
            Assert.IsNotNull(searcher, "Searcher can't be null");

            VoxelSearchContext context = ThreadSafePool<VoxelSearchContext>.Get(); 
            try
            {
                context.SetStartPosition(start);
                context.SetEndPosition(end);
                context.DistanceType = distanceType;
                context.Searcher = searcher;
                AStarPathFinding.FindPath(context, maxSearchrPocess);
                if (!context.Cancelled)
                {
                    searcher.OnPathFound(context.GetResult());
                }
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                context.CleanUp();
                ThreadSafePool<VoxelSearchContext>.Release(context);
            }
        }

        public VoxelSearchContext.Token FindPathAsync(ISearcher searcher, Vector3 start, Vector3 end)
        {
            Assert.IsNotNull(searcher, "Searcher can't be null");

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
                if(!context.Cancelled)
                {
                    searcher.OnPathFound(context.GetResult());
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                context.CleanUp();
                ThreadSafePool<VoxelSearchContext>.Release(context);
            }
        }
    }
}
