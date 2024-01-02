using CongTDev.Collection;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.ProceduralTerrain.Structures
{
    [CreateAssetMenu(menuName = "Minecraft/Structure/Tree")]
    public class TreeStructure_SO : Structure_SO
    {
        [SerializeField] private BlockType body;
        [SerializeField] private BlockType leaves;

        public override void GetModifications(MyNativeList<ModifierUnit> modifiers, Vector3Int position)
        {
            (int worldX, int worldY, int worldZ) = position;
            int size = 5;
            int whereLeavesStart = worldY + size;
            for (int y = worldY; y < whereLeavesStart; y++)
            {
                modifiers.Add(new ModifierUnit(worldX, y, worldZ, body));
            }
            int whereLeavesEnd = whereLeavesStart + size;
            size /= 2;
            for (int y = whereLeavesStart; y < whereLeavesEnd; y++)
            {
                for (int x = worldX - size; x <= worldX + size; x++)
                {
                    for (int z = worldZ - size; z <= worldZ + size; z++)
                    {
                        modifiers.Add(new ModifierUnit(x, y, z, leaves));
                    }
                }
            }
        }
    }
}