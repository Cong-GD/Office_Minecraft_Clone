using System;
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
        this.amount = Mathf.Max(0, amount);
    }

    public readonly bool IsEmpty()
    {
        return item == null || amount <= 0;
    }
}