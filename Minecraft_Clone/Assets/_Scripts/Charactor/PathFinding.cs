using CongTDev.AStarPathFinding;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft.AI
{
    public class VoxelNode : SearchNode<VoxelNode>
    {
        public int3 position;

        public VoxelNode(int3 position)
        {
            this.position = position;
        }
    }

    public class VoxelSearchContext : ISearchContext<VoxelNode>
    {
        public VoxelNode Start { get; set; }

        public VoxelNode End { get; set; }

        public int MaxNeightbours => 26;

        public List<VoxelNode> GeneratedPath { get; set; } = new();

        private Dictionary<int3, VoxelNode> _nodes = new();

        public VoxelSearchContext(Vector3 start, Vector3 end)
        {
            Start = GetNode((int3)math.floor(start));
            End = GetNode((int3)math.floor(end));
        }

        public uint Cost(VoxelNode from, VoxelNode to)
        {
            return (uint)math.round(math.distance(from.position, to.position) * 10f);
        }

        public Span<VoxelNode> GetNeighbours(VoxelNode node, Span<VoxelNode> buffer)
        {
            int count = 0;
            for (int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    for(int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;

                        int3 offset = new int3(x, y, z);
                        int3 neighbourPosition = node.position + offset;
                        if (CanWalk(node.position, neighbourPosition))
                        {
                            buffer[count] = GetNode(neighbourPosition);
                            count++;
                        }
                    }
                }
            }
            return buffer.Slice(0, count);
        }

        private bool CanWalk(int3 from, int3 to)
        {
            var block = Chunk.GetBlock(to).Data();
            to.y--;
            var blockBelow = Chunk.GetBlock(to).Data();
            to.y += 2;
            var blockAbove = Chunk.GetBlock(to).Data();
            return !block.IsSolid && blockBelow.IsSolid && !blockAbove.IsSolid;
        }

        public bool IsGoal(VoxelNode node)
        {
            return node.position.Equals(End.position);
        }

        private VoxelNode GetNode(int3 position)
        {
            if (_nodes.TryGetValue(position, out VoxelNode node))
            {
                return node;
            }

            node = new VoxelNode(position);
            _nodes.Add(position, node);
            return node;
        }

    }

    public class PathFinding : MonoBehaviour
    {
        public List<Vector3> FindPath(Vector3 start, Vector3 end)
        {
            VoxelSearchContext context = new VoxelSearchContext(start , end);

            AStarPathFinding.FindPath(context);
            return context.GeneratedPath.ConvertAll(node => (Vector3)((float3)node.position + new float3(0.5f, 0, 0.5f)));
        }
    }
}
