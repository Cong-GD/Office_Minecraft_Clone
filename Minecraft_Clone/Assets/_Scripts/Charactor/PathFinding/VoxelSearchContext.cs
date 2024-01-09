using CongTDev.AStarPathFinding;
using CongTDev.Collection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft.AI
{
    public enum DistanceType
    {
        SquaredEuclidean,
        Euclidean,
        Manhattan,
        Diagonal,
    }

    public class VoxelSearchContext : ISearchContext<VoxelNode>
    {
        public VoxelNode Start { get; private set; }

        public VoxelNode End { get; private set; }

        public VoxelNode CompletedAt { get; private set; }

        public DistanceType DistanceType { get; set; }

        public bool Cancelled { get; private set; }

        public bool Error { get; set; }

        public ISearcher Searcher { get; set; }

        public bool FlattenY { get; set; }

        public int MaxNeightbours => _direction3Ds.Length;

        private int _version;
        private readonly Dictionary<int3, VoxelNode> _nodeMap = new(5000);
        private readonly Stack<VoxelNode> _hasUse = new(5000);
        private readonly VoxelNode[] _neightbourBuffer = new VoxelNode[_direction3Ds.Length];

        private static readonly int3[] _direction3Ds =
        {
            new int3(-1, -1 , -1),
            new int3(-1, -1 , 0),
            new int3(-1, -1 , 1),
            new int3(-1, 0 , -1),
            new int3(-1, 0 , 0),
            new int3(-1, 0 , 1),
            new int3(-1, 1 , -1),
            new int3(-1, 1 , 0),
            new int3(-1, 1 , 1),
            new int3(0, -1 , -1),
            new int3(0, -1 , 0),
            new int3(0, -1 , 1),
            new int3(0, 0 , -1),
            new int3(0, 0 , 1),
            new int3(0, 1 , -1),
            new int3(0, 1 , 0),
            new int3(0, 1 , 1),
            new int3(1, -1 , -1),
            new int3(1, -1 , 0),
            new int3(1, -1 , 1),
            new int3(1, 0 , -1),
            new int3(1, 0 , 0),
            new int3(1, 0 , 1),
            new int3(1, 1 , -1),
            new int3(1, 1 , 0),
            new int3(1, 1 , 1)
        };

        public void SetStartPosition(Vector3 position)
        {
            int3 startPos = (int3)math.floor(position);
            Start = GetNode(startPos);
        }

        public void SetEndPosition(Vector3 position)
        {
            int3 endPos = (int3)math.floor(position);
            End = GetNode(endPos);
        }

        public uint CostBetween(VoxelNode from, VoxelNode to)
        {
            switch (DistanceType)
            {
                case DistanceType.Manhattan:
                    {
                        int dx = math.abs(from.position.x - to.position.x);
                        int dy = math.abs(from.position.y - to.position.y);
                        int dz = math.abs(from.position.z - to.position.z);
                        return (uint)(dx + dy + dz);
                    }
                case DistanceType.Diagonal:
                    {
                        const float D = 1f;
                        const float D2 = 1.4142135623730950488016887242097f;
                        float dx = math.abs(from.position.x - to.position.x);
                        float dy = math.abs(from.position.y - to.position.y);
                        float dz = math.abs(from.position.z - to.position.z);
                        return (uint)(D * (dx + dy + dz) + (D2 - 2 * D) * math.min(math.min(dx, dy), dz));
                    }
                case DistanceType.Euclidean:
                    {
                        int3 delta = from.position - to.position;
                        int distancesq = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                        return (uint)(math.sqrt(distancesq) * 10f);
                    }
                case DistanceType.SquaredEuclidean:
                default:
                    {
                        int3 delta = from.position - to.position;
                        float distancesq = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                        return (uint)distancesq;
                    }
            }
        }

        public ReadOnlySpan<VoxelNode> GetNeighbours(VoxelNode node)
        {
            int count = 0;
            int3[] direction3Ds = _direction3Ds;
            VoxelNode[] buffer = _neightbourBuffer;
            for (int i = 0; i < direction3Ds.Length; i++)
            {
                int3 neighbourPosition = node.position + direction3Ds[i];
                VoxelNode neightbourNode = GetNode(neighbourPosition);
                bool isClosed = neightbourNode.State == SearchState.Closed;
                if (!isClosed && Searcher.CanTraverse(new NodeProvider(this), node, neightbourNode))
                {
                    buffer[count++] = neightbourNode;
                }
            }
            return buffer.AsSpan(0, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsGoal(VoxelNode node)
        {
            if(FlattenY)
            {
                return node.position.x == End.position.x && node.position.z == End.position.z;
            }

            return node == End;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PathCompleteAt(VoxelNode node)
        {
            CompletedAt = node;
        }

        public void CleanUp()
        {
            _nodeMap.Clear();
            while (_hasUse.TryPop(out VoxelNode node))
            {
                node.State = SearchState.Unvisited;
                node.G = 0;
                node.H = 0;
                node.Conection = null;
                ThreadSafePool<VoxelNode>.Release(node);
            }
            Array.Clear(_neightbourBuffer, 0, _neightbourBuffer.Length);
            CompletedAt = null;
            _version++;
            Cancelled = false;
            Searcher = null;
            Error = false;
        }

        public CancelToken GetToken()
        {
            return new CancelToken(this);
        }

        public SearchResult GetResult()
        {
            return new SearchResult(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VoxelNode GetNode(int3 position)
        {
            if (_nodeMap.TryGetValue(position, out VoxelNode node))
            {
                return node;
            }

            node = ThreadSafePool<VoxelNode>.Get();
            _hasUse.Push(node);
            node.position = position;
            node.blockData = Chunk.GetBlock(position.x, position.y, position.z).Data();
            _nodeMap.Add(position, node);
            return node;
        }

        public readonly struct CancelToken
        {
            private readonly VoxelSearchContext _context;
            private readonly int _version;
            private readonly bool _hasValue;

            public CancelToken(VoxelSearchContext context)
            {
                _context = context;
                _hasValue = context is not null;
                _version = _hasValue ? context._version : 0;
            }

            public bool OnSearching
                => _hasValue && _version == _context._version && !_context.Cancelled;

            public void Cancel()
            {
                if (_hasValue && _version == _context._version)
                    _context.Cancelled = true;
            }
        }

        public readonly ref struct SearchResult
        {
            private readonly VoxelSearchContext _context;
            public SearchResult(VoxelSearchContext context)
            {
                _context = context;
            }

            public bool Error => _context.Error;

            public bool IsFound => _context.CompletedAt is not null;

            public Vector3[] GetPath()
            {
                VoxelNode node = _context.CompletedAt;
                int count = CountPathLength(node);
                if (count == 0)
                {
                    return Array.Empty<Vector3>();
                }

                Vector3[] path = new Vector3[count];
                for (int i = count - 1; i >= 0; i--)
                {
                    path[i] = new Vector3(
                        node.position.x + 0.5f,
                        node.position.y,
                        node.position.z + 0.5f);
                    node = node.Conection;
                }
                return path;
            }

            public void GetPath(MyNativeList<Vector3> path)
            {
                if (path is null)
                    throw new ArgumentNullException("List is null");

                path.Clear();
                VoxelNode node = _context.CompletedAt;
                while (node != null)
                {
                    path.Add(new Vector3(
                        node.position.x + 0.5f,
                        node.position.y,
                        node.position.z + 0.5f)
                        );
                    node = node.Conection;
                }
                path.Reverse();
            }

            public void GetPath(List<Vector3> path)
            {
                if (path is null)
                    throw new ArgumentNullException("List is null");

                path.Clear();
                VoxelNode node = _context.CompletedAt;
                while (node != null)
                {
                    path.Add(new Vector3(
                        node.position.x + 0.5f,
                        node.position.y,
                        node.position.z + 0.5f)
                        );
                    node = node.Conection;
                }
                path.Reverse();
            }

            private int CountPathLength(VoxelNode node)
            {
                int count = 0;
                while (node != null)
                {
                    count++;
                    node = node.Conection;
                }
                return count;
            }
        }

        public readonly ref struct NodeProvider
        {
            private readonly VoxelSearchContext _context;

            public NodeProvider(VoxelSearchContext context)
            {
                _context = context;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NodeView GetNode(int x, int y, int z)
            {
                return _context.GetNode(new int3(x, y, z));
            }
        }

    }
}
