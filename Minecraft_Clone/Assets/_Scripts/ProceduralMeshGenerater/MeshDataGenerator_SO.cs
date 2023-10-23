using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace My.GenerateMeshMethod
{
    public abstract class MeshDataGenerator_SO : ScriptableObject
    {
        public static readonly int AtlasSize = 16;
        public static readonly float Nomalized_UV_Value = 1f / AtlasSize;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetUVPosition(int uvIndex)
        {
            return new Vector2(
                x: uvIndex / AtlasSize * Nomalized_UV_Value,
                y: uvIndex % AtlasSize * Nomalized_UV_Value
                );
        }

        public abstract void GetMeshData(MeshData meshData, ChunkData chunkData, Vector3Int localPos);
    }
}
