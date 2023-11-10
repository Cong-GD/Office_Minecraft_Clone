using Minecraft.ProceduralMeshGenerate;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR;

public class InventorySystem : GlobalReference<InventorySystem>
{
    
    public readonly ItemSlot[] inventory = ItemUtilities.NewStogare(27);

    public readonly ItemSlot[] toolBarItems = ItemUtilities.NewStogare(9);

    [SerializeField]
    private ItemFactory_SO[] itemFactorys;

    public ItemSlot RightHand { get; private set; }

    public ItemSlot OffHand { get; private set; } = new ItemSlot();

    [SerializeField]
    private MinecraftObjectRenderer handRenderer;


    private void Start()
    {
        ItemSlot factorySlot = new ItemSlot();
        foreach (var factory in itemFactorys)
        {
            factorySlot.SetItem(factory.Create());
            AddItemToInventory(factorySlot);
        }
    }

    public void AddItemToInventory(ItemSlot source)
    {
        ItemUtilities.AddItem(toolBarItems, source);
        ItemUtilities.AddItem(inventory, source);
    }

    public void SetRightHand(ItemSlot hand)
    {
        if(RightHand is not null)
        {
            RightHand.OnItemModified -= RenderRightHand;
        }
        RightHand = hand;
        if(RightHand is not null)
        {
            RightHand.OnItemModified += RenderRightHand;
            RenderRightHand();
        }
    }

    private void RenderRightHand()
    {
        if(ItemSlot.IsNullOrEmpty(RightHand))
        {
            handRenderer.Clear();
            return;
        }
        handRenderer.RenderObject(RightHand.RootItem.GetObjectMeshData(), ItemTransformState.InRightHand);
    }
}
