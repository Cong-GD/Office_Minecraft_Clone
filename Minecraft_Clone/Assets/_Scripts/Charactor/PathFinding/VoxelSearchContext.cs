using CongTDev.AStarPathFinding;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft.AI
{
    public class VoxelNode : SearchNode<VoxelNode>
    {
        public int3 position;
        public BlockData_SO blockData;
    }

    public readonly struct NodeView
    {
        private readonly VoxelNode _voxelNode;

        public readonly int3 Position => _voxelNode.position;

        public readonly BlockData_SO BlockData => _voxelNode.blockData;

        public NodeView(VoxelNode voxelNode)
        {
            _voxelNode = voxelNode;
        }

        public static implicit operator NodeView(VoxelNode voxelNode)
        {
            return new NodeView(voxelNode);
        }
    }

    public interface ISearcher
    {
        bool CanTraverse(NodeView from, NodeView to);

        void OnPathFound(Vector3[] path);
    }

    public class VoxelSearchContext : ISearchContext<VoxelNode>
    {
        public VoxelNode Start { get; set; }

        public VoxelNode End { get; set; }

        public VoxelNode CompletedAt { get; private set; }

        public bool UseSqrtDistance { get; set; }

        public bool Cancelled { get; private set; }

        public ISearcher Searcher { get; set; }

        public int MaxNeightbours => _direction3Ds.Length;

        private int _version;
        private Dictionary<int3, VoxelNode> _nodes = new(10000);
        private Stack<VoxelNode> _hasUse = new(10000);
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
            int3 delta = to.position - from.position;
            int distancesq = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
            return UseSqrtDistance ? (uint)(Math.Sqrt(distancesq) * 10) : (uint)distancesq;
        }

        public ReadOnlySpan<VoxelNode> GetNeighbours(VoxelNode node, Span<VoxelNode> buffer)
        {
            int count = 0;
            int3[] direction3Ds = _direction3Ds;
            for (int i = 0; i < direction3Ds.Length; i++)
            {
                int3 neighbourPosition = node.position + direction3Ds[i];
                VoxelNode neightbourNode = GetNode(neighbourPosition);
                bool isClosed = neightbourNode.State == SearchState.Closed;
                if (!isClosed && Searcher.CanTraverse(node, neightbourNode))
                {
                    buffer[count++] = neightbourNode;
                }
            }
            return buffer.Slice(0, count);
        }

        public bool IsGoal(VoxelNode node)
        {
            return node == End;
        }

        public void PathCompleteAt(VoxelNode node)
        {
            CompletedAt = node;
        }

        public Vector3[] GetPath()
        {
            int count = CountPathLength(CompletedAt);
            if(count == 0)
            {
                return Array.Empty<Vector3>();
            }

            Vector3[] path = new Vector3[count];
            VoxelNode node = CompletedAt;
            for (int i = count - 1; i >= 0; i--)
            {
                path[i] = node.position + new float3(0.5f, 0f, 0.5f);
                node = node.Conection;
            }
            return path;
        }

        public void CleanUp()
        {
            while (_hasUse.TryPop(out VoxelNode node))
            {
                node.State = SearchState.Unvisited;
                node.G = 0;
                node.H = 0;
                node.Conection = null;
                ThreadSafePool<VoxelNode>.Release(node);
            }
            CompletedAt = null;
            _version++;
            Cancelled = false;
            Searcher = null;
            _nodes.Clear();
        }

        public CancelToken GetCancelToken()
        {
            return new CancelToken(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VoxelNode GetNode(int3 position)
        {
            if (_nodes.TryGetValue(position, out VoxelNode node))
            {
                return node;
            }

            node = ThreadSafePool<VoxelNode>.Get();
            _hasUse.Push(node);

            node.position = position;
            node.blockData = Chunk.GetBlock(position.x, position.y, position.z).Data();
            _nodes.Add(position, node);
            return node;
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

        public readonly struct CancelToken
        {
            private readonly VoxelSearchContext _context;
            private readonly int _version;

            public CancelToken(VoxelSearchContext context)
            {
                _context = context;
                _version = context._version;
            }

            public void Cancel()
            {
                if(_version == _context._version)
                    _context.Cancelled = true;
            }
        }

    }
}
