using System;
using System.Collections.Generic;

namespace Minecraft
{
    public struct LocalBlock : IEquatable<LocalBlock>
    {
        public byte x;
        public byte y;
        public byte z;
        public BlockType blockType;
        public Direction direction;

        public readonly bool Equals(LocalBlock other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public readonly override int GetHashCode()
        {
            return x | y << 8 | z << 16;
        }
    }
}