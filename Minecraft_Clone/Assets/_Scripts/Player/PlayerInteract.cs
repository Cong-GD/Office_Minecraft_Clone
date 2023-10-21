using System;
using System.Linq;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Inventory inventory;
    public Camera cam;

    public float checkDistance;

    bool destroyFound;
    bool placeFound;

    Vector3Int destroyPos;
    Vector3Int placePos;

    World world;

    private void Awake()
    {
        cam = Camera.main;
        world = FindAnyObjectByType<World>();
    }

    private void Update()
    {

        RayCast();

        if (Input.GetMouseButtonDown(0) && destroyFound)
        {
            world.EditBlock(destroyPos, BlockType.Air);
        }

        if (Input.GetMouseButtonDown(1) && placeFound && inventory.HandItem.Item is BlockData blockData)
        {
            world.EditBlock(placePos, blockData.blockType);
        }
    }

    private void RayCast()
    {
        var ray = new Ray(cam.transform.position, cam.transform.forward);

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