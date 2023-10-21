using System;

public static class ItemUtility
{
    public static ItemSlot[] NewStogare(int count)
    {
        ItemSlot[] storage = new ItemSlot[count];
        for (int i = 0; i < count; i++)
        {
            storage[i] = new ItemSlot();
        }
        return storage;
    }
}
