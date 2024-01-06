using CongTDev.Collection;
using Minecraft;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public enum State
    {
        None,
        Inventory,
        FullCraftingTable,
        BlastFurnace,
        Stogare
    }

    [SerializeField]
    private Canvas myCanvas;

    [SerializeField]
    private Canvas normalCharactorInfo;

    [SerializeField]
    private Canvas fullCraftingTable;

    [SerializeField]
    private Canvas stogare;

    [SerializeField]
    private UIFurnace uiFurnace;

    [SerializeField]
    private UIItemSlot[] toolBarSlots;

    [SerializeField]
    private Transform inventoryParent;

    [SerializeField]
    private UIItemSlot[] craftingSlot;

    [SerializeField]
    private UIItemSlot[] fullCraftingSlots;

    [SerializeField]
    private ResultUIItemSlot craftingResultSlot;

    [SerializeField]
    private ResultUIItemSlot fullCraftingResultSlot;

    [SerializeField]
    private UIItemSlot[] inventorySlots;

    [SerializeField]
    private List<UIItemSlot> storageSlots;

    [Header("Armor")]
    [SerializeField]
    private UIItemSlot helmetSlot;

    [SerializeField]
    private UIItemSlot chestplateSlot;

    [SerializeField]
    private UIItemSlot leggingsSlot;

    [SerializeField]
    private UIItemSlot boostsSlot;

    private readonly ManufactureSpace _manufactureSpace = new();
    private Stogare _stogare;

    private void Start()
    {
        var inventorySystem = InventorySystem.Instance;

        for (int i = 0; i < toolBarSlots.Length && i < inventorySystem.toolBarItems.Length; i++)
        {
            toolBarSlots[i].SetSlot(inventorySystem.toolBarItems[i]);
        }

        int count = Mathf.Min(inventorySlots.Length, inventorySystem.inventory.Length);
        for (int i = 0; i < count; i++)
        {
            inventorySlots[i].SetSlot(inventorySystem.inventory[i]);
        }

        SetupCraftingSlot();
        SetupArmorSlots();
    }

    public void SetState(State state)
    {
        myCanvas.enabled = state != State.None;
        uiFurnace.enabled = false;
        normalCharactorInfo.enabled = false;
        fullCraftingTable.enabled = false;
        stogare.enabled = false;

        switch (state)
        {
            case State.Inventory:
                normalCharactorInfo.enabled = true;
                break;
            case State.FullCraftingTable:
                fullCraftingTable.enabled = true;
                break;
            case State.BlastFurnace:
                uiFurnace.enabled = true;
                break;
            case State.Stogare:
                stogare.enabled = true;
                break;

        }
    }

    private void SetupCraftingSlot()
    {
        craftingSlot[0].Slot = _manufactureSpace.GetSlot(0, 0);
        craftingSlot[1].Slot = _manufactureSpace.GetSlot(1, 0);
        craftingSlot[2].Slot = _manufactureSpace.GetSlot(0, 1);
        craftingSlot[3].Slot = _manufactureSpace.GetSlot(1, 1);
        for (int i = 0; i < 9; i++)
        {
            fullCraftingSlots[i].Slot = _manufactureSpace.GetSlot(i);
        }

        craftingResultSlot.SetResultGiver(_manufactureSpace);
        fullCraftingResultSlot.SetResultGiver(_manufactureSpace);
    }

    private void SetupArmorSlots()
    {
        helmetSlot.SetSlot(InventorySystem.Instance.HelmetSlot);
        chestplateSlot.SetSlot(InventorySystem.Instance.ChestplateSlot);
        leggingsSlot.SetSlot(InventorySystem.Instance.LeggingSlot);
        boostsSlot.SetSlot(InventorySystem.Instance.BootsSlot);

    }

    public void SetFurnace(Furnace blastFurnace)
    {
        uiFurnace.SetFurnace(blastFurnace);
    }

    public void SetStogare(Stogare storage)
    {
        _stogare = storage;
        SetStogareSlot(storage.Slots);
    }
    
    private void SetStogareSlot(ReadOnlySpan<ItemSlot> slots)
    {
        int count = Mathf.Min(slots.Length, storageSlots.Count);
        int i = 0;
        for (; i < count; i++)
        {
            storageSlots[i].SetSlot(slots[i]);
        }
        for (; i < storageSlots.Count; i++)
        {
            storageSlots[i].ClearSlot();
        }
    }

    public void OnSortButtonClick()
    {
        _stogare?.Sort();
    }
}