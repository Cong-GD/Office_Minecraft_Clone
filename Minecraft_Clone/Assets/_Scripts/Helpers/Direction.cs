﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public enum Direction : byte
{
    Forward, Backward, Right, Left, Up, Down
}

public static class DirectionExtensions
{
    public static ReadOnlyCollection<Direction> SixDirections = new List<Direction>
    {
            Direction.Forward,
            Direction.Backward,
            Direction.Up,
            Direction.Down,
            Direction.Right,
            Direction.Left,
    }.AsReadOnly();
}
