using System;

public class ManufactureSpace
{
    public const int GRID_SIZE = 3;

    private readonly ItemSlot[] _materials = ItemUtilities.NewStogare(GRID_SIZE * GRID_SIZE);

    private bool _canCheckRecipe = true;

    public event Action<ItemPacked> OnCheckedResult;

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

    public ItemPacked TakeResult()
    {
        var pack = ItemUtilities.CheckRecipe(_materials);
        if (pack.IsEmpty())
            return pack;

        _canCheckRecipe = false;
        foreach (var slot in _materials)
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

        var pack = ItemUtilities.CheckRecipe(_materials);
        OnCheckedResult?.Invoke(pack);
    }
}
