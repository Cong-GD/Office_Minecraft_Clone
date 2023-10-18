using Unity.VisualScripting;
using UnityEngine;

public static class Vector3IntExtension
{

    public static void Parse(this Vector3Int vector, out int x, out int y, out int z)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
}