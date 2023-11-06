using Minecraft.Input;
using NaughtyAttributes;
using System;
using System.Linq;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField ,Required]
    private Inventory inventory;

    [SerializeField, Required]
    private Transform eye;

    [Min(1f)]
    public float checkDistance;

    bool destroyFound;
    bool placeFound;

    Vector3Int destroyPos;
    Vector3Int placePos;

    private void Update()
    {
        CheckForDestroy();
        CheckForPlaceBlock();
    }

    private void CheckForDestroy()
    {
        if (!MInput.LeftMouse.WasPerformedThisFrame())
            return;

        RayCast();
        if (!destroyFound)
            return;

        World.Instance.EditBlock(destroyPos, BlockType.Air);
    }

    private void CheckForPlaceBlock()
    {
        if (!MInput.RightMouse.WasPerformedThisFrame())
            return;

        RayCast();
        if (!placeFound || inventory.HandItem.Item is not BlockData_SO blockData)
            return;

        World.Instance.EditBlock(placePos, blockData.BlockType);
    }

    private void RayCast()
    {
        var ray = new Ray(eye.transform.position, eye.transform.forward);

        if (Physics.Raycast(ray, out var hit, checkDistance, LayerHelper.GroundLayer))
        {
            destroyPos = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);
            placePos = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);

            destroyFound = true;
            placeFound = true;
        }
        else
        {
            destroyFound = false;
            placeFound = false;
        }

    }
}