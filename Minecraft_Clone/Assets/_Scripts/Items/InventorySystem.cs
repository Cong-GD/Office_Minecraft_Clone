using CongTDev.Collection;
using Minecraft;
using Minecraft.ProceduralMeshGenerate;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : GlobalReference<InventorySystem>
{
    [SerializeField]
    private MinecraftObjectRenderer rightHandRenderer;

    [SerializeField]
    private MinecraftObjectRenderer leftHandRenderer;

    [SerializeField]
    private Armor playerArmor;

    [SerializeField]
    private Light torchLight;

    public readonly ItemSlot[] inventory = ItemUtilities.NewStogare(27);

    public readonly ItemSlot[] toolBarItems = ItemUtilities.NewStogare(9);

    public readonly ItemSlot HelmetSlot = new ItemSlot(new ArmorRequiment(ArmorType.Helmet));

    public readonly ItemSlot ChestplateSlot = new ItemSlot(new ArmorRequiment(ArmorType.Chestplate));

    public readonly ItemSlot LeggingSlot = new ItemSlot(new ArmorRequiment(ArmorType.Leggings));

    public readonly ItemSlot BootsSlot = new ItemSlot(new ArmorRequiment(ArmorType.Boots));

    public ItemSlot RightHand { get; private set; }

    public ItemSlot LeftHand { get; } = new ItemSlot();

    public ItemSlot OffHand { get; private set; } = new ItemSlot();

    private ArmorSource[] _armorSources;

    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.OnGameLoad += OnGameLoad;
        GameManager.Instance.OnGameSave += OnGameSave;
        _armorSources = new ArmorSource[]
        {
            new ArmorSource(HelmetSlot),
            new ArmorSource(ChestplateSlot),
            new ArmorSource(LeggingSlot),
            new ArmorSource(BootsSlot)
        };
        foreach (ArmorSource arrmorSource in _armorSources)
        {
            playerArmor.AddArmorSource(arrmorSource);
        }
        LeftHand.OnItemModified += RenderLeftHand;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameLoad -= OnGameLoad;
        GameManager.Instance.OnGameSave -= OnGameSave;
        foreach (ArmorSource arrmorSource in _armorSources)
        {
            playerArmor.RemoveArmorSource(arrmorSource);
        }
        LeftHand.OnItemModified -= RenderLeftHand;
    }

    private void OnGameSave(Dictionary<string, ByteString> byteDatas)
    {
        ByteString byteString = ByteString.Create(100);
        using ByteString inventory = ItemUtilities.ToByteString(this.inventory);
        using ByteString toolBarItems = ItemUtilities.ToByteString(this.toolBarItems);
        byteString.WriteByteString(inventory);
        byteString.WriteByteString(toolBarItems);
        LeftHand.GetPacked().WriteTo(byteString);
        HelmetSlot.GetPacked().WriteTo(byteString);
        ChestplateSlot.GetPacked().WriteTo(byteString);
        LeggingSlot.GetPacked().WriteTo(byteString);
        BootsSlot.GetPacked().WriteTo(byteString);
        byteDatas.Add("Inventory.dat", byteString);
    }

    private void OnGameLoad(Dictionary<string, ByteString> byteDatas)
    {
        try
        {
            if (byteDatas.Remove("Inventory.dat", out ByteString byteString))
            {
                ByteString.BytesReader byteReader = byteString.GetBytesReader();
                using ByteString inventoryData = byteReader.ReadByteString();
                using ByteString toolBarData = byteReader.ReadByteString();
                ItemUtilities.ParseToStogare(inventoryData, inventory);
                ItemUtilities.ParseToStogare(toolBarData, toolBarItems);
                LeftHand.SetItem(ItemPacked.ParseFrom(ref byteReader));
                HelmetSlot.SetItem(ItemPacked.ParseFrom(ref byteReader));
                ChestplateSlot.SetItem(ItemPacked.ParseFrom(ref byteReader));
                LeggingSlot.SetItem(ItemPacked.ParseFrom(ref byteReader));
                BootsSlot.SetItem(ItemPacked.ParseFrom(ref byteReader));
                byteString.Dispose();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
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
            rightHandRenderer.Clear();
            return;
        }
        rightHandRenderer.RenderObject(RightHand.RootItem.GetObjectMeshData(), ItemTransformState.InRightHand);
    }

    private void RenderLeftHand()
    {
        if(LeftHand.IsNullOrEmpty())
        {
            leftHandRenderer.Clear();
            torchLight.enabled = false;
            return;
        }
        leftHandRenderer.RenderObject(LeftHand.RootItem.GetObjectMeshData(), ItemTransformState.InRightHand);
        // Temporary solution for torch light
        torchLight.enabled = LeftHand.RootItem.Name == "Torch";
    }
}