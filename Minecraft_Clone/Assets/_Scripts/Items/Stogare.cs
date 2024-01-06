using CongTDev.Collection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public class Stogare : IBlockState
    {
        public Vector3Int Position { get; }

        private readonly ItemSlot[] _slots = ItemUtilities.NewStogare(27);

        public ReadOnlySpan<ItemSlot> Slots => _slots;

        private static readonly IComparer<ItemPacked> _comparer 
            = Comparer<ItemPacked>.Create((pack1, pack2) =>
            {
                bool isEmpty1 = pack1.IsEmpty();
                bool isEmpty2 = pack2.IsEmpty();
                if (isEmpty1 && isEmpty2)
                {
                    return 0;
                }
                else if (isEmpty1)
                {
                    return 1;
                }
                else if (isEmpty2)
                {
                    return -1;
                }
                else
                {
                    return pack1.item.Name.CompareTo(pack2.item.Name);
                }
            });

        public Stogare(Vector3Int position)
        {
            Position = position;
        }
        public Stogare(ref ByteString.BytesReader bytesReader)
        {
            Position = bytesReader.ReadValue<Vector3Int>();
            for (int i = 0; i < 27; i++)
            {
                _slots[i].SetItem(ItemPacked.ParseFrom(ref bytesReader));
            }
        }

        public void GetSerializedData(ByteString byteString)
        {
            byteString.WriteValue(Position);
            for (int i = 0; i < 27; i++)
            {
                _slots[i].GetPacked().WriteTo(byteString);
            }
        }

        public bool ValidateBlockState()
        {
            if(Chunk.GetBlock(Position) != BlockType.Barrel)
            {
                return false;
            }
            return true;
        }

        public void Sort()
        {
            ItemUtilities.SortStogare(_slots, _comparer);
        }
    }
}