using CongTDev.Collection;
using System;
using System.Text;
using UnityEngine;

[Serializable]
public struct ItemPacked
{
    public static readonly ItemPacked Empty = new (null, 0);

    public BaseItem_SO item;

    [Min(0)]
    public int amount;

    public ItemPacked(BaseItem_SO item, int amount = 1)
    {
        this.item = item;
        if(item == null)
        {
            this.amount = 0;
        }
        else
        {
            this.amount = Mathf.Clamp(amount, 0, item.MaxStack);
        }
    }

    public static ItemPacked ParseFrom(ref ByteString.BytesReader bytesReader)
    {
        bool isEmpty = bytesReader.ReadValue<bool>();
        if (isEmpty)
        {
            return Empty;
        }
        ReadOnlySpan<char> itemName = bytesReader.ReadChars();
        BaseItem_SO item = ItemUtilities.GetItemByName(itemName);
        int amount = bytesReader.ReadValue<int>();
        return new ItemPacked(item, amount);
    }

    public readonly void WriteTo(ByteString byteString)
    {
        bool isEmpty = IsEmpty();
        byteString.WriteValue(isEmpty);
        if (isEmpty)
        {
            return;
        }
        byteString.WriteChars(item.Name);
        byteString.WriteValue(amount);
    }

    public readonly bool IsEmpty()
    {
        return amount <= 0 || item == null; 
    }

    public readonly override string ToString()
    {
        return $"ItemPacked: {item.GetName()} x{amount}";
    }
}