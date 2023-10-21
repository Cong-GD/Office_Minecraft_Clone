using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private Transform toolBarParent;
    [SerializeField] private Transform inventoryParent;

    private UIItemSlot[] inventorySlots;
    private UIItemSlot[] toolBarSlot;

    private void Awake()
    {
        toolBarSlot = toolBarParent.GetComponentsInChildren<UIItemSlot>();
        for(int i = 0;i < toolBarSlot.Length && i < inventory.toolBarItems.Length;i++)
        {
            toolBarSlot[i].SetSlot(inventory.toolBarItems[i]);
        }

        inventorySlots = inventoryParent.GetComponentsInChildren<UIItemSlot>();
        for (int i = 0; i < inventorySlots.Length && i < inventory.inventory.Length; i++)
        {
            inventorySlots[i].SetSlot(inventory.inventory[i]);
        }
    }
}