using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.ProceduralTerrain.Structures
{
    [CreateAssetMenu(menuName = "Minecraft/Structure/Tree")]
    public class TreeStructure_SO : Structure_SO
    {
        //[SerializeField]
        //private int maxTreeSize = 10;

        //[SerializeField]
        //private int minTreeSize = 5;

        //[SerializeField]
        //private float noiseScale = 0.01f;

        [SerializeField] private BlockType body;
        [SerializeField] private BlockType leaves;

        public override void GetModifications(Queue<ModifierUnit> modifiers, Vector3Int position)
        {
            position.Parse(out int worldX, out int worldY, out int worldZ);
            //var noiseValue = Mathf.PerlinNoise(worldX + 0.1f * noiseScale, worldZ + 0.1f * noiseScale);
            int size = 5;//Mathf.RoundToInt(MyMath.RemapValue01(noiseValue, minTreeSize, maxTreeSize));
            int whereLeavesStart = worldY + size;
            for (int y = worldY; y < whereLeavesStart; y++)
            {
                modifiers.Enqueue(new ModifierUnit(worldX, y, worldZ, body));
            }
            int whereLeavesEnd = whereLeavesStart + size;
            size = size / 2;
            for (int y = whereLeavesStart; y < whereLeavesEnd; y++)
            {
                for (int x = worldX - size; x <= worldX + size; x++)
                {
                    for (int z = worldZ - size; z <= worldZ + size; z++)
                    {
                        modifiers.Enqueue(new ModifierUnit(x, y, z, leaves));
                    }
                }
                //size--;
            }
        }
    }
}