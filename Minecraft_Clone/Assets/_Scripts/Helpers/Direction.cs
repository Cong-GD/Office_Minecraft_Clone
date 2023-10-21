using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Forward, Backward, Right, Left, Up, Down
}

public static class DirectionExtensions
{
    public static IEnumerable<Direction> GetDirections()
    {
        yield return Direction.Forward;
        yield return Direction.Backward;
        yield return Direction.Up;
        yield return Direction.Down;
        yield return Direction.Right;
        yield return Direction.Left;
    }

    public static Vector3Int GetVector(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector3Int.up,
            Direction.Forward => Vector3Int.forward,
            Direction.Backward => Vector3Int.back,
            Direction.Down => Vector3Int.down,
            Direction.Left => Vector3Int.left,
            Direction.Right => Vector3Int.right,
            _ => throw new System.Exception("Invalid input direction")
        };
    }
}