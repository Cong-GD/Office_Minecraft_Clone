using NaughtyAttributes;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public enum State
    {
        None,
        Inventory,
        FullCraftingTable,
    }

    [SerializeField] 
    private Canvas renderingCanvas;

    [SerializeField] 
    private GameObject normalCharactorInfo;

    [SerializeField] 
    private GameObject fullCraftingTable;

    [SerializeField] 
    private Transform toolBarParent;

    [SerializeField] 
    private Transform inventoryParent;

    [SerializeField, BoxGroup("Crafting Slots")]
    private UIItemSlot craftingSlot00, craftingSlot10, craftingSlot01, craftingSlot11;

    [SerializeField]
    private UIItemSlot[] fullCraftingSlots;

    [SerializeField]
    private RecipeResultUIItemSlot craftingResultSlot;

    [SerializeField]
    private RecipeResultUIItemSlot fullCraftingResultSlot;

    private UIItemSlot[] inventorySlots;
    private UIItemSlot[] toolBarSlot;

    private ManufactureSpace _manufactureSpace = new();

    private void Awake()
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
        switch (state)
        {
            case State.None:
                renderingCanvas.enabled = false;
                break;
            case State.Inventory:
                renderingCanvas.enabled = true;
                normalCharactorInfo.SetActive(true);
                fullCraftingTable.SetActive(false);
                break;
            case State.FullCraftingTable:
                renderingCanvas.enabled = true;
                normalCharactorInfo.SetActive(false);
                fullCraftingTable.SetActive(true);
                break;
        }
    }

    private void SetupCraftingSlot()
    {
        craftingSlot00.Slot = _manufactureSpace.GetSlot(0, 0);
        craftingSlot10.Slot = _manufactureSpace.GetSlot(1, 0);
        craftingSlot01.Slot = _manufactureSpace.GetSlot(0, 1);
        craftingSlot11.Slot = _manufactureSpace.GetSlot(1, 1);
        for (int i = 0; i < ManufactureSpace.GRID_SIZE * ManufactureSpace.GRID_SIZE; i++)
        {
            fullCraftingSlots[i].Slot = _manufactureSpace.GetSlot(i);
        }

        craftingResultSlot.SetManufactureSpace(_manufactureSpace);
        fullCraftingResultSlot.SetManufactureSpace(_manufactureSpace);
    }
}