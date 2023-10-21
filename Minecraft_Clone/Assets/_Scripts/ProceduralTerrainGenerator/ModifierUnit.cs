using UnityEngine;

public readonly struct ModifierUnit
{
    public readonly int x; 
    public readonly int y; 
    public readonly int z;
    public readonly BlockType blockType;

    public readonly Vector3Int position => new Vector3Int(x, y, z);

    public ModifierUnit(Vector3Int position, BlockType blockType)
    {
        position.Parse(out x, out y, out z);
        this.blockType = blockType;
    }
}
