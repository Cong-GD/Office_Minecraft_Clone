using System;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft.Entity.Model
{
    [Serializable]
    public struct UvsPositionAndSize
    {
        public int2 position;
        public int2 size;
    }

    [Serializable]
    public class SingleBlockModelData
    {
        public int2 textureSize = 64;
        public float3 size;
        public float3 offset;

        [Header("Uvs position")]
        public UvsPositionAndSize left;
        public UvsPositionAndSize right;
        public UvsPositionAndSize up;
        public UvsPositionAndSize down;
        public UvsPositionAndSize front;
        public UvsPositionAndSize back;

        public bool drawCloth;
        public int2 clothUvsOffset;
        public float clothScale = 1.1f;
        public UvsPositionAndSize GetUvsPosition(Direction direction)
        {
            return direction switch
            {
                Direction.Left => left,
                Direction.Forward => front,
                Direction.Backward => back,
                Direction.Right => right,
                Direction.Up => up,
                Direction.Down => down,
                _ => throw new NotImplementedException()
            };
        }

    }
}


