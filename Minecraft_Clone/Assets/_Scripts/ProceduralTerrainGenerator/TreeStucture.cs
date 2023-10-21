using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Structure/Tree")]
public class TreeStucture : Structure
{
    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;

    [SerializeField] private BlockType body;
    [SerializeField] private BlockType leaves;

    public override IEnumerable<ModifierUnit> GetModifications(Vector3Int position)
    {
        int height = (int)(minTreeHeight + (maxTreeHeight - minTreeHeight) * Noise.Get2DPerlin(position.XZ(), 250f, 3f)) + position.y;
        for (; position.y < height; position.y++)
        {
            yield return new ModifierUnit(position, body);
        }

        for (int x = -2; x < 3; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = -2; z < 3; z++)
                {
                    yield return new ModifierUnit(position + new Vector3Int(x, y, z), leaves);
                }
            }
        }

    }
}