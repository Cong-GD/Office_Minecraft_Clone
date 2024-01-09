using FMODUnity;
using Minecraft.Audio;
using System;

public class ManufactureSpace : IResultGiver
{
    public const int GRID_SIZE = 3;

    private readonly ItemSlot[] _materials = ItemUtilities.NewStogare(GRID_SIZE * GRID_SIZE);

    private bool _canCheckRecipe = true;

    public event Action<ItemPacked> OnCheckedResult;

    private ItemPacked _currentResult;

    public ManufactureSpace()
    {
        for (int i = 0; i < GRID_SIZE * GRID_SIZE; i++)
        {
            _materials[i].OnItemModified += CheckForRecipe;
        }
    }

    ~ManufactureSpace()
    {
        for (int i = 0; i < GRID_SIZE * GRID_SIZE; i++)
        {
            _materials[i].OnItemModified -= CheckForRecipe;
        }
    }

    public int GetIndex(int x, int y) => y * GRID_SIZE + x;

    public ItemSlot GetSlot(int x, int y)
    {
        return _materials[GetIndex(x, y)];
    }

    public ItemSlot GetSlot(int index)
    {
        return _materials[index];
    }

    public ItemPacked PeekResult()
    {
        return _currentResult;
    }
    public ItemPacked TakeResult()
    {
        if (_currentResult.IsEmpty())
            return _currentResult;

        ItemPacked pack = _currentResult;
        _canCheckRecipe = false;
        foreach (ItemSlot slot in _materials)
        {
            slot.TakeAmount(1);
        }
        _canCheckRecipe = true;
        CheckForRecipe();
        return pack;
    }

    private void CheckForRecipe()
    {
        if (!_canCheckRecipe)
            return;

        _currentResult = ItemUtilities.CheckRecipe(_materials);
        OnCheckedResult?.Invoke(_currentResult);
    }
}
