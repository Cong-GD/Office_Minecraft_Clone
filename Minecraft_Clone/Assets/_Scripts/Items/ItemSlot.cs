using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IItem : IEquatable<IItem>
{
    public string Name { get; }
    public Sprite Icon { get; }

    public int MaxStack { get; }

}

public class ItemSlot
{

    public event Action OnItemChanged;
    public IItem Item { get; private set; }
    public int Amount { get; private set; }

    public bool IsEmpty()
    {
        return Item is null;
    }

    public bool TransferTo(ItemSlot slot, int amount)
    {
        if(slot is null)
        {
            throw new ArgumentNullException(nameof(slot));
        }

        if (amount < 1)
            return false;

        if(slot.IsEmpty())
        {
            TransferItemToEmptySlot(slot, amount);
            return true;
        }
        
        if(!Item.Equals(slot.Item))
            return false;

        TransferItemToSlotWithSameItem(slot, amount);
        return true;

    }

    private void TransferItemToEmptySlot(ItemSlot slot, int amount)
    {
        int gotAmount = TakeAmount(amount, out var item);
        slot.SetItem(item, gotAmount);
    }

    private void TransferItemToSlotWithSameItem(ItemSlot slot, int amount)
    {
        int gotAmount = TakeAmount(amount, out var item);
        slot.AddAmount(ref gotAmount);
        if (gotAmount < 1)
            return;

        if(IsEmpty())
        {
            SetItem(item, gotAmount);
            return;
        }
        AddAmount(ref gotAmount);
    }

    public void SetItem(IItem item, int amount) => SetItem(item, ref amount);

    public void SetItem(IItem item, ref int amount)
    {
        Item = item;
        Amount = 0;
        AddAmount(ref amount);
    }

    public bool AddAmount(ref int amount)
    {
        if (IsEmpty() || amount < 1)
        {
            return false;
        }

        int overload = Amount + amount - Item.MaxStack;
        if(overload < 1)
        {
            Amount += amount;
            amount = 0;
        }
        else
        {
            Amount = Item.MaxStack;
            amount = overload;
        }
        OnItemChanged?.Invoke();
        return true;
    }

    public int TakeAmount(int amount, out IItem item)
    {
        item = Item;
        if (IsEmpty() || amount < 1)
            return 0;

        int overload = Amount - amount;
        if(overload < 1)
        {
            Amount -= amount;
        }
        else
        {
            amount = Amount;
            Amount = 0;
        }
        if(Amount < 1)
        {
            Item = null;
        }

        OnItemChanged?.Invoke();
        return amount;
    }
    public void SwapItem(ItemSlot slot)
    {
        if(slot is null)
        {
            throw new ArgumentNullException(nameof(slot));
        }

        var item = slot.Item;
        var amount = slot.Amount;
        slot.Item = Item;
        slot.Amount = amount;
        Item = item;
        Amount = amount;
        OnItemChanged?.Invoke();
        slot.OnItemChanged?.Invoke();
    }
    
}
