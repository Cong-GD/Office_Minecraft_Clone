using UnityEngine;

[CreateAssetMenu(menuName = "Block Data")]
public class BlockData : ScriptableObject, IItem
{
    public BlockType blockType;

    public bool isSolid;
    public bool isTransparent;

    public bool generateCollider;

    [Header("Uv Index")]
    public int up;
    public int down;
    public int left;
    public int right;
    public int forward;
    public int backward;

    public string Name => name;

    [field: SerializeField]
    public Sprite Icon { get; private set;}

    [field: SerializeField]
    public int MaxStack { get; private set; } = 64;

    public bool Equals(IItem other)
    {
        if(other is not BlockData blockData)
            return false;
        return this == blockData;
    }

    public int GetUvIndex(Direction direction)
    {
        return direction switch
        {
            Direction.Up => up,
            Direction.Forward => forward,
            Direction.Backward => backward,
            Direction.Right => right,
            Direction.Left => left,
            Direction.Down => down,
            _ => 0
        };
    }

}
