using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Vector3;

public static class Vector3Extension
{
    private static readonly Vector3[] _fourDirections =
    {
        forward, back, right, left
    };

    private static readonly Vector3[] _sixDirections =
    {
        forward, back, right, left, up, down
    };

    public static IEnumerable<Vector3> FourDirections() => _fourDirections;

    public static IEnumerable<Vector3> SixDirections() => _sixDirections;
  
}
