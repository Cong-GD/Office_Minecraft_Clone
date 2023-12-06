using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Timeline;

public class UIInventory : MonoBehaviour
{
    public enum State
    {
        None,
        Inventory,
        FullCraftingTable,
        BlastFurnace
    }

    [SerializeField] 
    private Canvas renderingCanvas;

    [SerializeField] 
    private Canvas normalCharactorInfo;

    [SerializeField] 
    private Canvas fullCraftingTable;

    [SerializeField]
    private UIFurnace uiBlastFurnace;

    [SerializeField] 
    private Transform toolBarParent;

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

    private UIItemSlot[] inventorySlots;
    private UIItemSlot[] toolBarSlot;

    private ManufactureSpace _manufactureSpace = new();
    

    private void Start()
    {
        var inventorySystem = InventorySystem.Instance;

        toolBarSlot = toolBarParent.GetComponentsInChildren<UIItemSlot>();
        for(int i = 0;i < toolBarSlot.Length && i < inventorySystem.toolBarItems.Length;i++)
        {
            toolBarSlot[i].SetSlot(inventorySystem.toolBarItems[i]);
        }

        inventorySlots = inventoryParent.GetComponentsInChildren<UIItemSlot>();
        for (int i = 0; i < inventorySlots.Length && i < inventorySystem.inventory.Length; i++)
        {
            inventorySlots[i].SetSlot(inventorySystem.inventory[i]);
        }

        SetupCraftingSlot();
    }

    public void SetState(State state)
    {
        renderingCanvas.enabled = state != State.None;
        uiBlastFurnace.enabled = false;
        normalCharactorInfo.enabled = false;
        fullCraftingTable.enabled = false;

        switch (state)
        {
            case State.Inventory:
                normalCharactorInfo.enabled = true;
                break;
            case State.FullCraftingTable:
                fullCraftingTable.enabled = true;
                break;
            case State.BlastFurnace:
                uiBlastFurnace.enabled = true;
                break;
        }
    }

    private void SetupCraftingSlot()
    {
        craftingSlot[0].Slot = _manufactureSpace.GetSlot(0, 0);
        craftingSlot[1].Slot = _manufactureSpace.GetSlot(1, 0);
        craftingSlot[2].Slot = _manufactureSpace.GetSlot(0, 1);
        craftingSlot[3].Slot = _manufactureSpace.GetSlot(1, 1);
        for (int i = 0; i < ManufactureSpace.GRID_SIZE * ManufactureSpace.GRID_SIZE; i++)
        {
            fullCraftingSlots[i].Slot = _manufactureSpace.GetSlot(i);
        }

        craftingResultSlot.SetResultGiver(_manufactureSpace);
        fullCraftingResultSlot.SetResultGiver(_manufactureSpace);
    }

    internal void SetFurnace(Furnace blastFurnace)
    {
        uiBlastFurnace.SetFurnace(blastFurnace);
    }
}