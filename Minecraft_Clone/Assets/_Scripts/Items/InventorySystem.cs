using Minecraft.ProceduralMeshGenerate;
using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class InventorySystem : GlobalReference<InventorySystem>
{
    
    public readonly ItemSlot[] inventory = ItemUtility.NewStogare(27);

    public readonly ItemSlot[] toolBarItems = ItemUtility.NewStogare(9);


    [SerializeField]
    private ItemFactory_SO[] itemFactorys;

    public ItemSlot RightHand { get; private set; }

    public ItemSlot OffHand { get; private set; } = new ItemSlot();

    [SerializeField]
    private MinecraftObjectRenderer handHolder;

    [field: SerializeField]
    public ItemDragingSystem DragingSystem { get; private set; }

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
        ItemUtility.AddItem(toolBarItems, source);
        ItemUtility.AddItem(inventory, source);
    }

    public void SetHand(ItemSlot hand)
    {
        RightHand = hand;
        if (hand is null || hand.IsEmpty())
        {
            handHolder.Clear();
            return;
        }

        handHolder.RenderObject(hand.RootItem.GetObjectMeshData(), ItemTransformState.InRightHand);
    }
}
