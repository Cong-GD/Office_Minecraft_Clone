using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int XZ(this Vector3Int v)
    {
        return new Vector2Int(v.x, v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int X_Z(this Vector3Int v, int y)
    {
        return new Vector3Int(v.x, y, v.z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int X_Y(this Vector2Int v, int y)
    {
        return new Vector3Int(v.x, y, v.y);
    }

    public static void Parse(this Vector3Int vector, out int x, out int y, out int z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}