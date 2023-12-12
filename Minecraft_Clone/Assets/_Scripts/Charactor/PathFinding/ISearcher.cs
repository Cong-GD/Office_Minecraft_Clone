using Unity.Mathematics;
using UnityEngine;

namespace Minecraft.AI
{
    public interface ISearcher
    {
        bool CanTraverse(IContext context, NodeView from, NodeView to);

        void OnPathFound(VoxelSearchContext.SearchResult result);
    }

    public interface IContext
    {
        public NodeView GetNode(int x, int y, int z);
    }

}
