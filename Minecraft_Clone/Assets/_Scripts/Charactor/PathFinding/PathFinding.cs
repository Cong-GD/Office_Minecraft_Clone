using CongTDev.AStarPathFinding;
using System.Threading.Tasks;
using UnityEngine;

namespace Minecraft.AI
{
    public class PathFinding : MonoBehaviour
    {
        [SerializeField, Min(10)]
        private int maxSearchrPocess = 10000;

        private VoxelSearchContext _context = new VoxelSearchContext();

        public bool useSqrtDistance = true;

        private void Awake()
        {
            ThreadSafePool<VoxelNode>.Capacity = 100000;
        }

        public void FindPath(ISearcher searcher, Vector3 start, Vector3 end)
        {
            _context.SetStartPosition(start);
            _context.SetEndPosition(end);
            _context.UseSqrtDistance = useSqrtDistance;
            _context.Searcher = searcher;
            AStarPathFinding.FindPath(_context, maxSearchrPocess);
            Vector3[] path = _context.GetPath();
            _context.CleanUp();
            searcher.OnPathFound(path);
        }

        public VoxelSearchContext.CancelToken FindPathAsync(ISearcher searcher, Vector3 start, Vector3 end)
        {
            _context.SetStartPosition(start);
            _context.SetEndPosition(end);
            _context.UseSqrtDistance = useSqrtDistance;
            _context.Searcher = searcher;
            FindPathAsyncInternal(_context, searcher);
            return _context.GetCancelToken();
        }

        private async void FindPathAsyncInternal(VoxelSearchContext context, ISearcher searcher)
        {
            Vector3[] path = await Task.Run(() =>
            {
                AStarPathFinding.FindPath(context, maxSearchrPocess);
                path = context.GetPath();
                context.CleanUp();
                return path;
            });
            searcher.OnPathFound(path);
        }
    }
}
