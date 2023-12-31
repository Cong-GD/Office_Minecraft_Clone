﻿using Minecraft;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/Block/Crafting Table")]
public class CraftingTable_SO : BlockData_SO, IInteractableBlock
{
    public void Interact(Vector3Int worldPosition)
    {
        UIManager.Instance.OpenCraftingTable();
    }
}
