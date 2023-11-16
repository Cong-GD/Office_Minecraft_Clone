using System;

public static class CollectionHelper
{
    public static void Clear<T>(this Span<T> span)
    {
        for (int i = 0; i < span.Length; i++)
        {
            span[i] = default;
        }
    }
}