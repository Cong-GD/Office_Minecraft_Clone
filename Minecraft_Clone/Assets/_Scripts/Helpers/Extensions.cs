using Unity.VisualScripting;
using UnityEngine;

public static class Extensions
{
    public static Vector2Int XZ(this Vector3Int vector)
    {
        return new Vector2Int(vector.x, vector.z);
    }

    public static void Parse(this Vector3Int vector, out int x, out int y, out int z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}