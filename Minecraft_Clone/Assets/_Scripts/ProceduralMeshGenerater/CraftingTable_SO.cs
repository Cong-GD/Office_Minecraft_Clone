using Minecraft;
using UnityEngine;

public class CraftingTable_SO : BlockData_SO, IInteractable, IBurnAbleItem
{
    [field: SerializeField]
    public float BurnDuration { get; private set; }

    public void Interact()
    {
        UIManager.Instance.OpenCraftingTable();
    }
}
