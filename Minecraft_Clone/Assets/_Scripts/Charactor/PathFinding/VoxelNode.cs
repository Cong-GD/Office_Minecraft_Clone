using CongTDev.AStarPathFinding;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Minecraft.AI
{
    public class VoxelNode : SearchNode<VoxelNode>
    {
        public int3 position;
        public BlockData_SO blockData;
    }

    public readonly ref struct NodeView
    {
        private readonly VoxelNode _voxelNode;
     
        public readonly int3 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _voxelNode.position;
        }

        public readonly BlockData_SO BlockData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _voxelNode.blockData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NodeView(VoxelNode voxelNode)
        {
            _voxelNode = voxelNode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator NodeView(VoxelNode voxelNode)
        {
            return new NodeView(voxelNode);
        }
    }
}
