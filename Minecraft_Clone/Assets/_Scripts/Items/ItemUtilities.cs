﻿using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public static class ItemUtilities
{
    private static Dictionary<int3x3, Recipe_SO> _recipes;

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
        Debug.Log("Recipe generated: " + _recipes.Count);
    }

    public static ItemPacked CheckRecipe(ReadOnlySpan<ItemSlot> slots)
    {
        if(slots.Length != 9)
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

    public static bool HasAnyItem(ReadOnlySpan<ItemSlot> slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty())
                return true;
        }
        return false;
    }

    public static ItemSlot[] NewStogare(int count)
    {
        if(count <= 0)
            return Array.Empty<ItemSlot>();

        ItemSlot[] storage = new ItemSlot[count];
        for (int i = 0; i < count; i++)
        {
            storage[i] = new ItemSlot();
        }
        return storage;
    }

    public static ItemSlot[] NewStogare(int count, IItemSlotRequiment slotRequiment)
    {
        if (count <= 0)
            return Array.Empty<ItemSlot>();

        ItemSlot[] storage = new ItemSlot[count];
        for (int i = 0; i < count; i++)
        {
            storage[i] = new ItemSlot(slotRequiment);
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
}