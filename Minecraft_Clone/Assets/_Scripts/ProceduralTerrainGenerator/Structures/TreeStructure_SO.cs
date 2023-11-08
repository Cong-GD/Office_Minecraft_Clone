using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.ProceduralTerrain.Structures
{
    [CreateAssetMenu(menuName = "Minecraft/Structure/Tree")]
    public class TreeStructure_SO : Structure_SO
    {
        public int maxTreeHeight = 12;
        public int minTreeHeight = 5;

        [SerializeField] private BlockType body;
        [SerializeField] private BlockType leaves;

        public override void GetModifications(Queue<ModifierUnit> modifiers, Vector3Int position)
        {
            int height = position.y + 5;//(int)(minTreeHeight + (maxTreeHeight - minTreeHeight) * Noise.Get2DPerlin(position.XZ(), 250f, 3f)) + position.y;
            for (int y = position.y; y < height; y++)
            {
                modifiers.Enqueue(new ModifierUnit(position.x, y, position.z, body));
            }
            int endX = position.x + 3;
            int endY = height + 4;
            int endZ = position.z + 3;
            for (int x = position.x - 2; x < endX; x++)
            {
                for (int y = height; y < endY; y++)
                {
                    for (int z = position.z - 2; z < endZ; z++)
                    {
                        modifiers.Enqueue(new ModifierUnit(x, y, z, leaves));
                    }
                }
            }

        }
    }
}