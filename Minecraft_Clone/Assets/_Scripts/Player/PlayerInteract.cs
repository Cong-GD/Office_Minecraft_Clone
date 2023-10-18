using System;
using System.Linq;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Camera cam;

    public float checkDistance;

    bool destroyFound;
    bool placeFound;

    Vector3Int destroyPos;
    Vector3Int placePos;

    World world;
    public int selectedBlock = 1;

    private BlockType[] blockTypes = Enum.GetValues(typeof(BlockType)).Cast<BlockType>().ToArray();


    private void Awake()
    {
        cam = Camera.main;
        world = FindAnyObjectByType<World>();
    }

    private void Update()
    {

        RayCast();
        SelecteBlock();

        if (Input.GetMouseButtonDown(0) && destroyFound)
        {
            world.EditBlock(destroyPos, BlockType.Air);
        }

        if (Input.GetMouseButtonDown(1) && placeFound)
        {
            world.EditBlock(placePos, blockTypes[selectedBlock]);
        }
    }

    private void SelecteBlock()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            if (scroll > 0)
            {
                selectedBlock++;
            }
            else
            {
                selectedBlock--;
            }
            if(selectedBlock < 1)
            {
                selectedBlock += (blockTypes.Length - 1);
            }
            selectedBlock %= blockTypes.Length;
            Debug.Log(blockTypes[selectedBlock]);
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