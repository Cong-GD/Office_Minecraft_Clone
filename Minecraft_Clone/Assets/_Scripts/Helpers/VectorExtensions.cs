using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class VectorExtensions
{

    public readonly static ReadOnlyCollection<Vector3Int> SixDirectionsVector3Int = new List<Vector3Int>
    {
            new Vector3Int( 0,  0,  1),
            new Vector3Int( 0,  0, -1),
            new Vector3Int( 0,  1,  0),
            new Vector3Int( 0, -1,  0),
            new Vector3Int( 1,  0,  0),
            new Vector3Int(-1,  0,  0),
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
    public static Vector3Int With(this Vector3Int v, int? x = null, int? y = null, int? z = null)
    {
        return new Vector3Int(x ?? v.x,y ?? v.y, z ?? v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 With(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? v.x, y ?? v.y, z ?? v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Add(this Vector3Int v, int? x = null, int? y = null, int? z = null)
    {
        return new Vector3Int(
            v.x + x.GetValueOrDefault(),
            v.y + y.GetValueOrDefault(),
            v.z + z.GetValueOrDefault());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Add(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(
            v.x + x.GetValueOrDefault(), 
            v.y + y.GetValueOrDefault(), 
            v.z + z.GetValueOrDefault());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int Multily(this Vector3Int v, int? x = null, int? y = null, int? z = null)
    {
        return new Vector3Int(
            v.x * x.GetValueOrDefault(1),
            v.y * y.GetValueOrDefault(1),
            v.z * z.GetValueOrDefault(1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Multily(this Vector3 v, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(
            v.x * x.GetValueOrDefault(1f),
            v.y * y.GetValueOrDefault(1f),
            v.z * z.GetValueOrDefault(1f));
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

    public static void Deconstruct(this Vector3Int v, out int x, out int y, out int z)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public static void Deconstruct(this int3 v, out int x, out int y, out int z)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }
}
