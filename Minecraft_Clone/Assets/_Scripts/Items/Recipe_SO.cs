using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/Recipes")]
public class Recipe_SO : ScriptableObject
{
    [Serializable]
    public class ItemBindings
    {
        [Range(1, 9)]
        public int id = 1;
        public BaseItem_SO item;
    }

    [Serializable]
    public class RecipeBinding
    {
        public Vector3Int row2;
        public Vector3Int row1;
        public Vector3Int row0;

        public RecipeBinding Clone()
        {
            var binding = new RecipeBinding();
            binding.row0 = row0;
            binding.row1 = row1;
            binding.row2 = row2;
            return binding;
        }
        
    }

    [SerializeField]
    private List<ItemBindings> itemBindings;

    [SerializeField]
    private ItemPacked result;

    [SerializeField]
    private RecipeBinding recipeBinding;


    public ItemPacked GetResult() => result;

    public IEnumerable<string> GetRecipeBindings()
    {
        ThrowIfInvalidBinding(recipeBinding);
        var binding = recipeBinding.Clone();
        while (MoveDown(binding));
        while (MoveLeft(binding));

        foreach (var hashString in CheckRow(binding))
        {
            yield return hashString;
        }
        while(MoveUp(binding))
        {
            foreach (var hashString in CheckRow(binding))
            {
                yield return hashString;
            }
        }
    }

    private IEnumerable<string> CheckRow(RecipeBinding binding)
    {
        binding = binding.Clone();
        yield return GetHashString(binding);
        while(MoveRight(binding))
        {
            yield return GetHashString(binding);
        }
    }

    private bool MoveRight(RecipeBinding binding)
    {
        ThrowIfInvalidBinding(binding);
        if (binding.row0.z != 0 || binding.row1.z != 0 || binding.row2.z != 0)
            return false;

        binding.row0.z = binding.row0.y;
        binding.row1.z = binding.row1.y;
        binding.row2.z = binding.row2.y;

        binding.row0.y = binding.row0.x;
        binding.row1.y = binding.row1.x;
        binding.row2.y = binding.row2.x;

        binding.row0.x = 0;
        binding.row1.x = 0;
        binding.row2.x = 0;
        return true;
    }

    private bool MoveLeft(RecipeBinding binding)
    {
        ThrowIfInvalidBinding(binding);
        if (binding.row0.x != 0 || binding.row1.x != 0 || binding.row2.x != 0)
            return false;

        binding.row0.x = binding.row0.y;
        binding.row1.x = binding.row1.y;
        binding.row2.x = binding.row2.y;

        binding.row0.y = binding.row0.z;
        binding.row1.y = binding.row1.z;
        binding.row2.y = binding.row2.z;

        binding.row0.z = 0;
        binding.row1.z = 0;
        binding.row2.z = 0;
        return true;
    }

    private bool MoveUp(RecipeBinding binding)
    {
        ThrowIfInvalidBinding(binding);
        if (binding.row2 != Vector3Int.zero)
            return false;

        binding.row2 = binding.row1;
        binding.row1 = binding.row0;
        binding.row0 = Vector3Int.zero;
        return true;
    }

    private bool MoveDown(RecipeBinding binding)
    {
        ThrowIfInvalidBinding(binding);
        if (binding.row0 != Vector3Int.zero)
            return false;

        binding.row0 = binding.row1;
        binding.row1 = binding.row2;
        binding.row2 = Vector3Int.zero;
        return true;
    }

    private string GetHashString(RecipeBinding recipeBindings)
    {
        return 
            $"{ToHashString(recipeBindings.row0.x)}" +
            $",{ToHashString(recipeBindings.row0.y)}" +
            $",{ToHashString(recipeBindings.row0.z)}" +
            $",{ToHashString(recipeBindings.row1.x)}" +
            $",{ToHashString(recipeBindings.row1.y)}" +
            $",{ToHashString(recipeBindings.row1.z)}" +
            $",{ToHashString(recipeBindings.row2.x)}" +
            $",{ToHashString(recipeBindings.row2.y)}" +
            $",{ToHashString(recipeBindings.row2.z)}";
    }

    private string ToHashString(int itemId)
    {
        var binding = itemBindings.Find(x => x.id == itemId);
        return binding == null ? "(null)" : binding.item.GetName();
    }

    private void ThrowIfInvalidBinding(RecipeBinding recipeBinding)
    {
        if (recipeBinding.row0 == Vector3Int.zero
          && recipeBinding.row1 == Vector3Int.zero
          && recipeBinding.row2 == Vector3Int.zero)
            throw new Exception($"Recipe binding can't be empty: {name}");
    }

}