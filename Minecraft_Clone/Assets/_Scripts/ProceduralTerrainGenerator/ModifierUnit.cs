using UnityEngine;

public readonly struct ModifierUnit
{
    public readonly int x; 
    public readonly int y; 
    public readonly int z;
    public readonly BlockType blockType;

    public readonly Vector3Int GetPosition() => new Vector3Int(x, y, z);

    public ModifierUnit(int x, int y, int z, BlockType blockType)
    {
        this.x = x; 
        this.y = y;
        this.z = z;
        this.blockType = blockType;
    }

    public ModifierUnit(Vector3Int position, BlockType blockType)
    {
        position.Parse(out x, out y, out z);
        this.blockType = blockType;
    }
}
