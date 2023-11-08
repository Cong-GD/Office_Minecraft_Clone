public readonly struct ItemPacked
{
    public static readonly ItemPacked Empty = new ItemPacked(null, 0);

    public readonly BaseItem_SO item;
    public readonly int amount;

    public ItemPacked(BaseItem_SO item, int amount = 1)
    {
        this.item = item;
        this.amount = amount;
    }

    public bool IsEmpty()
    {
        return item == null || amount <= 0;
    }
}