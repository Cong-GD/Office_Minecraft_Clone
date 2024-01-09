using CongTDev.Collection;
using ObjectPooling;
using UnityEngine;

namespace Minecraft.AI
{
    public abstract class BaseMonster : PoolObject
    {
        [field: SerializeField]
        public Health Health { get; private set; }

        [field: SerializeField]
        public ItemPacked DropItem { get; private set; }

        public abstract ByteString ToByteString();

        public abstract void FromByteString(ByteString byteString);

        public abstract void SetPosition(Vector3 position);

    }
}
