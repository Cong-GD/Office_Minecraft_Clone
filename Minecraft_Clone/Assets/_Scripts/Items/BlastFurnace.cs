using System;
using System.Collections;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UIElements;

public class BlastFurnace : IResultGiver, IBlockState
{
    private class BurnAbleRequiment : IItemSlotRequiment
    {
        public bool CheckRequiment(BaseItem_SO item) => item is IBurnAbleItem;
    }

    private class CookAbleRequiment : IItemSlotRequiment
    {
        public bool CheckRequiment(BaseItem_SO item) => item is ICookAbleItem;
    }

    private static readonly IItemSlotRequiment _cachedBurnAbleRequiment = new BurnAbleRequiment();
    private static readonly IItemSlotRequiment _cachedCookAbleRequiment = new CookAbleRequiment();

    public event Action<ItemPacked> OnCheckedResult;

    public readonly ItemSlot cookSlot = new (_cachedCookAbleRequiment);

    public readonly ItemSlot burnSlot = new (_cachedBurnAbleRequiment);


    public float BurnProgressValue => IsBurning ? (Time.time - _startBurnTime) / _burnTime : 0f;
    public float CookProgressValue => IsCooking ? (Time.time - _startCookTime) / _cookTime : 0f;

    public bool IsBurning { get; private set; }
    public bool IsCooking { get; private set; }


    private float _startBurnTime;
    private float _startCookTime;
    private float _burnTime;
    private float _cookTime;

    private BaseItem_SO _cookingItem;
    private readonly ItemSlot _resultSlot = new();

    private Coroutine _cookingCoroutine;
    private Coroutine _burningCoroutine;
    private Vector3Int _position;

    public BlastFurnace(Vector3Int position)
    {
        cookSlot.OnItemModified += ValidateState;
        burnSlot.OnItemModified += ValidateState;
        _position = position;
    }

    ~BlastFurnace()
    {
        CoroutineHelper.Stop(_cookingCoroutine);
        CoroutineHelper.Stop(_burningCoroutine);
        cookSlot.OnItemModified -= ValidateState;
        burnSlot.OnItemModified -= ValidateState;
    }

    public bool Validate()
    {
        if(Chunk.GetBlock(_position) != BlockType.Furnace)
        {
            Debug.Log("A furnace has been destroyed");

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

    private void CancelCooking()
    {
        if (!IsCooking)
            return;

        IsCooking = false;
        CoroutineHelper.Stop(_cookingCoroutine);
    }


    private void CheckForBurning()
    {
        if(!IsBurning && !burnSlot.IsEmpty() && IsValidCookMaterial())
        {
            CoroutineHelper.Start(BurningCoroutine(), out _burningCoroutine);
        }
    }

    private void CheckForCooking()
    {
        if(IsCooking)
        {
            if (!IsBurning || !IsValidCookMaterial())
                CancelCooking();
        }
        else if(IsBurning && IsValidCookMaterial())
        {
            CoroutineHelper.Start(CookingCoroutine(), out _cookingCoroutine);
        }
    }

    private bool IsValidCookMaterial()
    {
        if (cookSlot.IsEmpty())
            return false;

        if (_resultSlot.IsEmpty())
            return true;

        var resultPack = ((ICookAbleItem)cookSlot.RootItem).CookResult;
        if (_resultSlot.RootItem != resultPack.item)
            return false;

        var available = _resultSlot.RootItem.MaxStack - _resultSlot.Amount;
        return available >= resultPack.amount;
    }

    private IEnumerator BurningCoroutine()
    {
        IsBurning = true;
        var burnItem = burnSlot.TakeAmount(1).item;
        _burnTime = ((IBurnAbleItem)burnItem).BurnDuration;
        _startBurnTime = Time.time;
        yield return Wait.ForSeconds(_burnTime);
        IsBurning = false;
        ValidateState();
    }

    private IEnumerator CookingCoroutine()
    {
        _cookingItem = cookSlot.RootItem;
        _cookTime = ((ICookAbleItem)_cookingItem).CookDuration;
        _startCookTime = Time.time;
        IsCooking = true;
        yield return Wait.ForSeconds(_cookTime);
        IsCooking = false;

        var cookResult = ((ICookAbleItem)_cookingItem).CookResult;
        cookSlot.TakeAmount(1);
        if (_resultSlot.IsEmpty())
        {
            _resultSlot.SetItem(cookResult);
        }
        else if(_resultSlot.RootItem == cookResult.item)
        {
            _resultSlot.AddAmount(ref cookResult.amount);
        }   
        OnCheckedResult?.Invoke(_resultSlot.GetPacked());
        ValidateState();
    }
}
