using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [SerializeField] private Transform toolBarParent;
    [SerializeField] private Transform inventoryParent;

    private UIItemSlot[] inventorySlots;
    private UIItemSlot[] toolBarSlot;

    [SerializeField]
    private RecipeResultUIItemSlot recipeResultSlot;

    [SerializeField]
    private UIItemSlot slot00, slot01, slot10, slot11;

    private ManufactureSpace _manufactureSpace = new();

    private void Awake()
    {
        var inventorySystem = InventorySystem.Instance;

        // initialize toolbar slots;
        toolBarSlot = toolBarParent.GetComponentsInChildren<UIItemSlot>();
        for(int i = 0;i < toolBarSlot.Length && i < inventorySystem.toolBarItems.Length;i++)
        {
            toolBarSlot[i].SetSlot(inventorySystem.toolBarItems[i]);
        }

        // initialize inventory slots;
        inventorySlots = inventoryParent.GetComponentsInChildren<UIItemSlot>();
        for (int i = 0; i < inventorySlots.Length && i < inventorySystem.inventory.Length; i++)
        {
            inventorySlots[i].SetSlot(inventorySystem.inventory[i]);
        }

        // initialize manufacture space;
        slot00.Slot = _manufactureSpace.GetSlot(0, 0);
        slot01.Slot = _manufactureSpace.GetSlot(0, 1);
        slot10.Slot = _manufactureSpace.GetSlot(1, 0);
        slot11.Slot = _manufactureSpace.GetSlot(1, 1);

        recipeResultSlot.SetManufactureSpace(_manufactureSpace);
    }
}