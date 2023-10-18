using UnityEngine;

[CreateAssetMenu(menuName = "Block Data")]
public class BlockDataSO : ScriptableObject
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
