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
        if(item == null)
        {
            this.amount = 0;
        }
        else
        {
            this.amount = Mathf.Clamp(amount, 0, item.MaxStack);
        }
        
    }

    public readonly bool IsEmpty()
    {
        return amount <= 0 || item == null; 
    }
}