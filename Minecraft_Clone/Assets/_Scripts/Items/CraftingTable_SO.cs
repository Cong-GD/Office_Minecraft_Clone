using Minecraft;
using UnityEngine;

public class CraftingTable_SO : BlockData_SO, IInteractable
{
    public void Interact(Vector3Int worldPosition)
    {
        UIManager.Instance.OpenCraftingTable();
    }
}
