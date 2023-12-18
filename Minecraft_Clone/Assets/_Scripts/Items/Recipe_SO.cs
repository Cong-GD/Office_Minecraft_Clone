using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/Recipes")]
public class Recipe_SO : ScriptableObject
{
    private const int GRID_SIZE = 3;

    [Serializable]
    public class ItemBindings
    {
        [Range(1, 9)]
        public int id = 1;
        public BaseItem_SO item;
    }

    [SerializeField]
    private List<ItemBindings> itemBindings;

    [SerializeField]
    private ItemPacked result;

    [SerializeField]
    private int3x3 recipeBinding;

    public ItemPacked GetResult() => result;

    public IEnumerable<int3x3> GetRecipeBindings()
    {
        int3x3 binding = ToItemIDs(ref recipeBinding);
        ThrowIfInvalidBinding(ref binding);
        while (MoveDown(ref binding));
        while (MoveLeft(ref binding));

        do
        {
            int3x3 copy = binding;
            do
            {
                yield return copy;
            }
            while (MoveRight(ref copy));
        }
        while(MoveUp(ref binding));
    }

    private bool MoveRight(ref int3x3 binding)
    {
        ThrowIfInvalidBinding(ref binding);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (binding[i][2] != 0)
                return false;
        }

        for (int i = 0; i < GRID_SIZE; i++)
        {
            binding[i][2] = binding[i][1];
            binding[i][1] = binding[i][0];
            binding[i][0] = 0;
        }
        return true;
    }

    private bool MoveLeft(ref int3x3 binding)
    {
        ThrowIfInvalidBinding(ref binding);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (binding[i][0] != 0)
                return false;
        }

        for (int i = 0; i < GRID_SIZE; i++)
        {
            binding[i][0] = binding[i][1];
            binding[i][1] = binding[i][2];
            binding[i][2] = 0;
        }

        return true;
    }

    private bool MoveUp(ref int3x3 binding)
    {
        ThrowIfInvalidBinding(ref binding);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (binding[0][i] != 0)
                return false;
        }

        for (int i = 0; i < GRID_SIZE; i++)
        {
            binding[0][i] = binding[1][i];
            binding[1][i] = binding[2][i];
            binding[2][i] = 0;
        }
        return true;
    }

    private bool MoveDown(ref int3x3 binding)
    {
        ThrowIfInvalidBinding(ref binding);
        for (int i = 0; i < GRID_SIZE; i++)
        {
            if (binding[2][i] != 0)
                return false;
        }

        for (int i = 0; i < GRID_SIZE; i++)
        {
            binding[2][i] = binding[1][i];
            binding[1][i] = binding[0][i];
            binding[0][i] = 0;
        }
        return true;
    }

    private int3x3 ToItemIDs(ref int3x3 binding)
    {
        return new int3x3(
            ToItemID(binding[0][2]), ToItemID(binding[1][2]), ToItemID(binding[2][2]),
            ToItemID(binding[0][1]), ToItemID(binding[1][1]), ToItemID(binding[2][1]),
            ToItemID(binding[0][0]), ToItemID(binding[1][0]), ToItemID(binding[2][0])
            );
    }

    private int ToItemID(int bindingID)
    {
        var binding = itemBindings.Find(x => x.id == bindingID);
        return binding == null ? 0 : binding.item.GetItemID();
    }

    private void ThrowIfInvalidBinding(ref int3x3 recipeBinding)
    {
        if (recipeBinding.Equals(0))
            throw new Exception($"Recipe binding can't be empty: {name}");
    }

}