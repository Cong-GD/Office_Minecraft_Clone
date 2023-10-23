using My.GenerateMeshMethod;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Block Data")]
public class BlockData_SO : ScriptableObject, IItem
{
    [field: SerializeField] public BlockType blockType { get; private set; }

    [field: SerializeField] public bool isSolid { get; private set; }

    [field: SerializeField] public bool isTransparent { get; private set; }

    [field: SerializeField] public Sprite Icon { get; private set; }

    [field: SerializeField] public int MaxStack { get; private set; } = 64;

    [field: SerializeField] public MeshDataGenerator_SO MeshGenerator { get; private set; }

    [Header("Uv Index")]
    public int up;
    public int down;
    public int left;
    public int right;
    public int forward;
    public int backward;

    public string Name => name;

    public bool Equals(IItem other)
    {
        if(other is not BlockData_SO blockData)
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