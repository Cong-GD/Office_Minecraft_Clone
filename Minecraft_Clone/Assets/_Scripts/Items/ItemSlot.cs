using System;

public class ItemSlot
{

    public event Action OnItemModified;
    public BaseItem_SO RootItem { get; private set; }
    public int Amount { get; private set; }

    public bool IsFullStacked => !IsEmpty() && Amount >= RootItem.MaxStack;

    private readonly IItemSlotRequiment _slotRequiment;

    public ItemSlot()
    {
        _slotRequiment = IItemSlotRequiment.Empty;
    }

    public ItemSlot(IItemSlotRequiment slotRequiment)
    {
        if (slotRequiment is null)
            throw new ArgumentNullException("Slot requiment");

        _slotRequiment = slotRequiment;
    }

    public bool IsEmpty()
    {
        return Amount == 0 || RootItem == null;
    }

    public bool IsMeetSlotRequiment(BaseItem_SO item)
    {
        return item == null || _slotRequiment.CheckRequiment(item);
    }

    public bool TryTransferTo(ItemSlot slot, int amount)
    {
        if (slot is null)
        {
            throw new ArgumentNullException(nameof(slot));
        }

        if (amount < 1)
            return false;

        if (!slot.IsMeetSlotRequiment(RootItem))
            return false;

        if (slot.IsEmpty())
        {
            TransferItemToEmptySlot(slot, amount);
            return true;
        }

        if (RootItem != slot.RootItem || slot.IsFullStacked)
            return false;

        TransferItemToSlotWithSameItem(slot, amount);
        return true;

    }

    private void TransferItemToEmptySlot(ItemSlot slot, int amount)
    {
        slot.SetItem(TakeAmount(amount));
    }

    private void TransferItemToSlotWithSameItem(ItemSlot slot, int amount)
    {
        int gotAmount = TakeAmount(amount, out var item);
        slot.AddAmount(ref gotAmount);
        if (gotAmount < 1)
            return;

        if (IsEmpty())
        {
            SetItem(item, gotAmount);
            return;
        }
        AddAmount(ref gotAmount);
    }

    public ItemPacked GetPacked()
    {
        return new ItemPacked(RootItem, Amount);
    }

    public bool SetItem(ItemPacked itemPacked)
    {
        return SetItem(itemPacked.item, itemPacked.amount);
    }

    public bool SetItem(BaseItem_SO item, int amount = 1)
    {
        if (!IsMeetSlotRequiment(item))
            return false;

        if (item == null || amount < 1)
        {
            RootItem = null;
            Amount = 0;
        }
        else
        {
            RootItem = item;
            Amount = amount;
        }
        OnItemModified?.Invoke();
        return true;
    }

    public bool AddAmount(ref int amount)
    {
        if (IsEmpty() || amount < 1)
        {
            return false;
        }

        int overload = Amount + amount - RootItem.MaxStack;
        if (overload < 1)
        {
            Amount += amount;
            amount = 0;
        }
        else
        {
            Amount = RootItem.MaxStack;
            amount = overload;
        }
        OnItemModified?.Invoke();
        return true;
    }

    public ItemPacked TakeAmount(int amount)
    {
        amount = TakeAmount(amount, out var item);
        return new ItemPacked(item, amount);
    }

    public int TakeAmount(int amount, out BaseItem_SO item)
    {
        item = RootItem;
        if (IsEmpty() || amount < 1)
            return 0;

        if (amount >= Amount)
        {
            amount = Amount;
            RootItem = null;
            Amount = 0;
            OnItemModified?.Invoke();
            return amount;
        }

        Amount -= amount;
        OnItemModified?.Invoke();
        return amount;
    }
    public void SwapItem(ItemSlot slot)
    {
        if (slot is null)
        {
            throw new ArgumentNullException(nameof(slot));
        }

        if (!slot.IsMeetSlotRequiment(RootItem) || !IsMeetSlotRequiment(slot.RootItem))
            return;

        var item = slot.RootItem;
        var amount = slot.Amount;
        slot.RootItem = RootItem;
        slot.Amount = Amount;
        RootItem = item;
        Amount = amount;
        OnItemModified?.Invoke();
        slot.OnItemModified?.Invoke();
    }

}
