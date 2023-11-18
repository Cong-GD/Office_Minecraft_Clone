﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public static class ItemUtilities
{
    private static Dictionary<string, Recipe_SO> _recipes;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        using var timer = TimeExcute.Start("Intialized all recipes");
        var recipes = Resources.LoadAll<Recipe_SO>("Recipes");
        _recipes = new Dictionary<string, Recipe_SO>();
        foreach (var recipe in recipes)
        {
            foreach (var binding in recipe.GetRecipeBindings())
            {
                if (!_recipes.TryAdd(binding, recipe))
                {
                    Debug.LogError($"Can't have duplicate recipe: {_recipes[binding].name} <-> {recipe.name}");
                    Debug.LogError($"Binding: {binding}");
                }
            }
        }
        Debug.Log("Recipe generated: " + _recipes.Count);
    }

    public static ItemPacked CheckRecipe(ItemSlot[] slots)
    {
        if (slots.Length != 9)
            throw new Exception("Manufacture space must have 9 slots");

        string hashString = string.Join(",", 
            slots.Select(slot => slot.RootItem.GetName()));

        if(_recipes.TryGetValue(hashString, out var recipe))
        {
            return recipe.GetResult();
        }
        return ItemPacked.Empty;
    }

    public static bool HasAnyItem(Span<ItemSlot> slots)
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
        ItemSlot[] storage = new ItemSlot[count];
        for (int i = 0; i < count; i++)
        {
            storage[i] = new ItemSlot();
        }
        return storage;
    }

    public static ItemSlot[] NewStogare(int count, IItemSlotRequiment slotRequiment)
    {
        ItemSlot[] storage = new ItemSlot[count];
        for (int i = 0; i < count; i++)
        {
            storage[i] = new ItemSlot(slotRequiment);
        }
        return storage;
    }

    public static void AddItem(Span<ItemSlot> target, ItemSlot source)
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
}
