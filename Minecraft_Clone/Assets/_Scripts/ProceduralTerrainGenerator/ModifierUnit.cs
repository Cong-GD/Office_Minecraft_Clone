using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Minecraft
{
    public readonly struct ModifierUnit
    {
        public readonly int x;
        public readonly int y;
        public readonly int z;
        public readonly BlockType blockType;
        public readonly Direction direction;

        public ModifierUnit(int x, int y, int z, BlockType blockType, Direction direction = Direction.Forward)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.blockType = blockType;
            this.direction = direction;
        }
    }
}