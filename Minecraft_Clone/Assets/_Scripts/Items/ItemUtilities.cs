using CongTDev.Collection;
using Minecraft.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public static class ItemUtilities
{
    private static Dictionary<int3x3, Recipe_SO> _recipes;

    private static BaseItem_SO[] _allItems;

    public static ReadOnlySpan<BaseItem_SO> AllItems => _allItems;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Recipe_SO[] recipes = Resources.LoadAll<Recipe_SO>("Recipes");
        _recipes = new Dictionary<int3x3, Recipe_SO>();
        foreach (Recipe_SO recipe in recipes)
        {
            try
            {
                foreach (int3x3 binding in recipe.GetRecipeBindings())
                {
                    if (!_recipes.TryAdd(binding, recipe))
                    {
                        Debug.LogError($"Can't have duplicate recipe: {_recipes[binding].name} <-> {recipe.name}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error when load recipe: {recipe.name}\n{e}");
            }
        }
        Debug.Log("Recipes generated: " + _recipes.Count);

        _allItems = Resources.LoadAll<BaseItem_SO>("Items").Where(item => item.IsValidItem).ToArray();
        Array.Sort(_allItems, (item1, item2) => item1.Name.AsSpan().CompareTo(item2.Name.AsSpan(), StringComparison.Ordinal));
        Debug.Log("Items loaded: " + _allItems.Length);
    }

    public static BaseItem_SO GetItemByName(ReadOnlySpan<char> name)
    {
        if(name.IsEmpty)
        {
            return null;
        }

        int left = 0;
        int right = _allItems.Length - 1;
        while (left <= right)
        {
            int mid = (left + right) / 2;
            int compareResult = _allItems[mid].Name.AsSpan().CompareTo(name, StringComparison.Ordinal);
            if (compareResult == 0)
            {
                return _allItems[mid];
            }
            else if (compareResult < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return null;
    }

    public static ItemPacked CheckRecipe(ReadOnlySpan<ItemSlot> slots)
    {
        if (slots.Length != 9)
            throw new ArgumentException("Manufacture space must have 9 slots");

        int3x3 hashCodes = new int3x3(
            GetItemID(slots[0].RootItem), GetItemID(slots[1].RootItem), GetItemID(slots[2].RootItem),
            GetItemID(slots[3].RootItem), GetItemID(slots[4].RootItem), GetItemID(slots[5].RootItem),
            GetItemID(slots[6].RootItem), GetItemID(slots[7].RootItem), GetItemID(slots[8].RootItem)
            );

        if (_recipes.TryGetValue(hashCodes, out Recipe_SO recipe))
        {
            return recipe.GetResult();
        }
        return ItemPacked.Empty;
    }

    public static ItemSlot[] NewStogare(int count)
    {
        if (count <= 0)
            return Array.Empty<ItemSlot>();

        ItemSlot[] storage = new ItemSlot[count];
        for (int i = 0; i < count; i++)
        {
            storage[i] = new ItemSlot();
        }
        return storage;
    }

    public static void AddItem(ReadOnlySpan<ItemSlot> target, ItemSlot source)
    {
        if (source.IsEmpty())
            return;

        foreach (ItemSlot slot in target)
        {
            source.TryTransferTo(slot, source.Amount);
            if (source.IsEmpty())
                return;
        }
    }

    public static string GetName(this BaseItem_SO item)
    {
        return item == null ? "(null)" : item.Name;
    }

    public static bool IsNullOrEmpty(this ItemSlot slot)
    {
        return slot is null || slot.IsEmpty();
    }

    public static int GetItemID(this BaseItem_SO item)
    {
        return item == null ? 0 : item.GetInstanceID();
    }

    public static void SortStogare(ReadOnlySpan<ItemSlot> stogare, IComparer<ItemPacked> comparer)
    {
        try
        {
            List<ItemPacked> packed = new List<ItemPacked>(stogare.Length);
            foreach (ItemSlot slot in stogare)
            {
                packed.Add(slot.TakeAmount(slot.Amount));
            }
            packed.Sort(comparer);
            ItemSlot holder = new ItemSlot();
            foreach (ItemPacked item in packed)
            {
                holder.SetItem(item);
                AddItem(stogare, holder);
            }
        }
        catch (Exception)
        {
            Debug.LogWarning("Fail to sort stogare");
        }
    }

    public static ByteString ToByteString(ReadOnlySpan<ItemSlot> stogare)
    {
        ByteString byteString = ByteString.Create(stogare.Length * 25);
        byteString.WriteValue(stogare.Length);
        foreach (ItemSlot slot in stogare)
        {
            slot.GetPacked().WriteTo(byteString);
        }
        return byteString;
    }

    public static void ParseToStogare(ByteString byteString, ReadOnlySpan<ItemSlot> stogare)
    {
        ByteString.BytesReader bytesReader = byteString.GetBytesReader();
        int count = bytesReader.ReadValue<int>();
        count = math.min(count, stogare.Length);
        int i = 0;
        for (; i < count; i++)
        {
            stogare[i].SetItem(ItemPacked.ParseFrom(ref bytesReader));
        }
        for (; i < stogare.Length; i++)
        {
            stogare[i].SetItem(ItemPacked.Empty);
        }
    }
}