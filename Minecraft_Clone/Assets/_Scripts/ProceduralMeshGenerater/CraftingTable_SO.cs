using Minecraft;

public class CraftingTable_SO : BlockData_SO, IInteractable
{
    public void Interact()
    {
        UIManager.Instance.OpenCraftingTable();
    }
}
