using System;
using System.Collections;
using UnityEngine;

public class BlastFurnace : IResultGiver, IBlockState
{
    private class BurnAbleRequiment : IItemSlotRequiment
    {
        public bool CheckRequiment(BaseItem_SO item) => item.CanBurn;
    }

    private class SmeltAbleRequiment : IItemSlotRequiment
    {
        public bool CheckRequiment(BaseItem_SO item) => item.CanSmelt;
    }

    private static readonly IItemSlotRequiment _cachedBurnAbleRequiment = new BurnAbleRequiment();
    private static readonly IItemSlotRequiment _cachedSmeltAbleRequiment = new SmeltAbleRequiment();

    public event Action<ItemPacked> OnCheckedResult;

    public readonly ItemSlot smeltSlot = new(_cachedSmeltAbleRequiment);

    public readonly ItemSlot burnSlot = new(_cachedBurnAbleRequiment);


    public float BurnProgressValue => IsBurning ? 1f - (Time.time - _startBurnTime) / _burnTime : 0f;
    public float SmeltProgressValue => IsSmelting ? (Time.time - _startCookTime) / _cookTime : 0f;

    public bool IsBurning { get; private set; }
    public bool IsSmelting { get; private set; }


    private float _startBurnTime;
    private float _startCookTime;
    private float _burnTime;
    private float _cookTime;

    private BaseItem_SO _smeltItem;
    private readonly ItemSlot _resultSlot = new();

    private Coroutine _smeltCoroutine;
    private Coroutine _burningCoroutine;
    private Vector3Int _position;

    public BlastFurnace(Vector3Int position)
    {
        smeltSlot.OnItemModified += ValidateState;
        burnSlot.OnItemModified += ValidateState;
        _position = position;
    }

    ~BlastFurnace()
    {
        CoroutineHelper.Stop(_smeltCoroutine);
        CoroutineHelper.Stop(_burningCoroutine);
        smeltSlot.OnItemModified -= ValidateState;
        burnSlot.OnItemModified -= ValidateState;
    }

    public bool Validate()
    {
        if (Chunk.GetBlock(_position) != BlockType.Furnace)
        {
            Debug.Log("A furnace has been destroyed");
            // Do something on desroy
            return false;
        }
        return true;
    }

    public ItemPacked PeekResult()
    {
        return _resultSlot.GetPacked();
    }

    public ItemPacked TakeResult()
    {
        var result = _resultSlot.TakeAmount(_resultSlot.Amount);
        ValidateState();
        return result;
    }

    private void ValidateState()
    {
        CheckForBurning();
        CheckForCooking();
    }

    private void CancelSmelting()
    {
        if (!IsSmelting)
            return;

        IsSmelting = false;
        CoroutineHelper.Stop(_smeltCoroutine);
    }


    private void CheckForBurning()
    {
        if (!IsBurning && !burnSlot.IsEmpty() && IsValidSmeltMaterial())
        {
            CoroutineHelper.Start(BurningCoroutine(), out _burningCoroutine);
        }
    }

    private void CheckForCooking()
    {
        if (IsSmelting)
        {
            if (!IsBurning || !IsValidSmeltMaterial())
                CancelSmelting();
        }
        else if (IsBurning && IsValidSmeltMaterial())
        {
            CoroutineHelper.Start(SmeltingCoroutine(), out _smeltCoroutine);
        }
    }

    private bool IsValidSmeltMaterial()
    {
        if (smeltSlot.IsEmpty())
            return false;

        if (_resultSlot.IsEmpty())
            return true;

        var resultPack = smeltSlot.RootItem.SmeltResult;
        if (_resultSlot.RootItem != resultPack.item)
            return false;

        var available = _resultSlot.RootItem.MaxStack - _resultSlot.Amount;
        return available >= resultPack.amount;
    }

    private IEnumerator BurningCoroutine()
    {
        IsBurning = true;
        var burnItem = burnSlot.TakeAmount(1).item;
        _burnTime = burnItem.BurnDuration;
        _startBurnTime = Time.time;
        yield return Wait.ForSeconds(_burnTime + 0.05f);
        IsBurning = false;
        ValidateState();
    }

    private IEnumerator SmeltingCoroutine()
    {
        _smeltItem = smeltSlot.RootItem;
        _cookTime = _smeltItem.SmeltDuration;
        _startCookTime = Time.time;
        IsSmelting = true;
        yield return Wait.ForSeconds(_cookTime);
        IsSmelting = false;

        var cookResult = _smeltItem.SmeltResult;
        smeltSlot.TakeAmount(1);
        if (_resultSlot.IsEmpty())
        {
            _resultSlot.SetItem(cookResult);
        }
        else if (_resultSlot.RootItem == cookResult.item)
        {
            _resultSlot.AddAmount(ref cookResult.amount);
        }
        OnCheckedResult?.Invoke(_resultSlot.GetPacked());
        ValidateState();
    }
}
