using System;

public interface IResultGiver
{
    event Action<ItemPacked> OnCheckedResult;

    ItemPacked PeekResult();

    ItemPacked TakeResult();
}
