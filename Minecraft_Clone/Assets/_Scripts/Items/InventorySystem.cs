using Minecraft.ProceduralMeshGenerate;
using UnityEngine;

public class InventorySystem : GlobalReference<InventorySystem>
{

    public readonly ItemSlot[] inventory = ItemUtilities.NewStogare(27);

    public readonly ItemSlot[] toolBarItems = ItemUtilities.NewStogare(9);

    [SerializeField]
    private ItemPacked[] startupItem;

    public ItemSlot RightHand { get; private set; }

    public ItemSlot OffHand { get; private set; } = new ItemSlot();

    [SerializeField]
    private MinecraftObjectRenderer handRenderer;


    private void Start()
    {
        ItemSlot slot = new ItemSlot();
        foreach (ItemPacked itemPacked in startupItem)
        {
            slot.SetItem(itemPacked);
            AddItemToInventory(slot);
        }
    }

    public void AddItemToInventory(ItemSlot source)
    {
        ItemUtilities.AddItem(toolBarItems, source);
        ItemUtilities.AddItem(inventory, source);
    }

    public void SetRightHand(ItemSlot hand)
    {
        if (RightHand is not null)
        {
            RightHand.OnItemModified -= RenderRightHand;
        }
        RightHand = hand;
        if (RightHand is not null)
        {
            RightHand.OnItemModified += RenderRightHand;
            RenderRightHand();
        }
    }

    private void RenderRightHand()
    {
        if (RightHand.IsNullOrEmpty())
        {
            handRenderer.Clear();
            return;
        }
        handRenderer.RenderObject(RightHand.RootItem.GetObjectMeshData(), ItemTransformState.InRightHand);
    }
}
