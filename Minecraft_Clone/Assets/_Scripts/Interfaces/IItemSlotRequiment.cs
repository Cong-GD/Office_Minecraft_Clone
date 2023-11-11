public interface IItemSlotRequiment
{
    public readonly static IItemSlotRequiment Empty = new EmptyRequiment();

    bool CheckRequiment(BaseItem_SO item);
}

public class EmptyRequiment : IItemSlotRequiment
{
    public bool CheckRequiment(BaseItem_SO item)
    {
        return true;
    }
}