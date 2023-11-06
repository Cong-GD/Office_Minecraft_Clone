using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using UnityEngine;


public static class VectorExtensions
{
    public readonly static ReadOnlyCollection<Vector2Int> EightDirectionsVector2Int = new List<Vector2Int>
    {
        new Vector2Int( 0,  1),
        new Vector2Int( 1,  1),
        new Vector2Int( 1,  0),
        new Vector2Int( 1, -1),
        new Vector2Int( 0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1,  0),
    }.AsReadOnly();

    public readonly static ReadOnlyCollection<Vector3Int> SixDirectionsVector3Int = new List<Vector3Int>
    {
            new Vector3Int( 0,  0,  1),
            new Vector3Int( 0,  0, -1),
            new Vector3Int( 0,  1,  0),
            new Vector3Int( 0, -1,  0),
            new Vector3Int( 1,  0,  0),
            new Vector3Int(-1,  0,  0),
    }.AsReadOnly();

    public readonly static ReadOnlyCollection<Vector3> SixDirectionsVector3 = new List<Vector3>
    {
            new Vector3( 0,  0,  1),
            new Vector3( 0,  0, -1),
            new Vector3( 0,  1,  0),
            new Vector3( 0, -1,  0),
            new Vector3( 1,  0,  0),
            new Vector3(-1,  0,  0),
    }.AsReadOnly();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int XZ(this Vector3Int v)
    {
        return new Vector2Int(v.x, v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int X_Z(this Vector3Int v, int y)
    {
        return new Vector3Int(v.x, y, v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 X_Z(this Vector3 v, float y)
    {
        return new Vector3(v.x, y, v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int X_Y(this Vector2Int v, int y)
    {
        return new Vector3Int(v.x, y, v.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 X_Y(this Vector2 v, float y)
    {
        return new Vector3(v.x, y, v.y);
    }

    public static void Parse(this Vector3Int vector, out int x, out int y, out int z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public static void Parse(this Vector3 vector, out float x, out float y, out float z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}
