using Minecraft.Input;
using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField, Required]
    private Transform eye;

    [Min(1f)]
    public float checkDistance;

    [SerializeField]
    private LayerMask entityLayer;

    [SerializeField]
    private LayerMask groundLayer;

    private bool destroyFound;
    private bool placeFound;

    private Vector3Int destroyPos;
    private Vector3Int placePos;

    private readonly Vector3 _halfOne = new Vector3(0.5f, 0.5f, 0.5f);

    private void OnEnable()
    {
        MInput.LeftMouse.performed += OnLeftClicked;
        MInput.RightMouse.performed += OnRightClicked;
        MInput.Throw.performed += ProcessThrowInput;
    }

    private void OnDisable()
    {
        MInput.LeftMouse.performed -= OnLeftClicked;
        MInput.RightMouse.performed -= OnRightClicked;
        MInput.Throw.performed -= ProcessThrowInput;
    }

    private void OnRightClicked(InputAction.CallbackContext context)
    {
        CheckForPlaceBlock();
    }

    private void OnLeftClicked(InputAction.CallbackContext context)
    {
        CheckForDestroy();
    }

    private async void CheckForDestroy()
    {
        RayCast();
        if (!destroyFound)
            return;

        var block = Chunk.GetBlock(destroyPos).Data();
        if (block.BlockType == BlockType.Air)
            return;

        var isSucceed = await World.Instance.EditBlockAsync(destroyPos, BlockType.Air);
        if(isSucceed)
        {
            FreeMinecraftObject.ThrowItem(new(block, 1), destroyPos, Vector3.zero);
        }
    }

    private async void CheckForPlaceBlock()
    {
        var rightHand = InventorySystem.Instance.RightHand;
        if (ItemSlot.IsNullOrEmpty(rightHand))
            return;

        RayCast();
        if (!placeFound || rightHand.RootItem is not BlockData_SO blockData)
            return;

        if (Physics.CheckBox(placePos + _halfOne, _halfOne, Quaternion.identity, entityLayer))
            return;
        if (!World.Instance.CanEdit())
            return;

        rightHand.TakeAmount(1, out _);
        await World.Instance.EditBlockAsync(placePos, blockData.BlockType);
    }

    private void ProcessThrowInput(InputAction.CallbackContext obj)
    {
        if (ItemSlot.IsNullOrEmpty(InventorySystem.Instance.RightHand))
            return;

        var itemPacked = InventorySystem.Instance.RightHand.TakeAmount(1);
        FreeMinecraftObject.ThrowItem(itemPacked, eye.position + eye.forward, eye.forward * 1.5f);
    }


    private void RayCast()
    {
        var ray = new Ray(eye.transform.position, eye.transform.forward);

        if (Physics.Raycast(ray, out var hit, checkDistance, groundLayer))
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

    private void OnDrawGizmosSelected()
    {
        RayCast();
        if (placeFound)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(placePos + _halfOne, Vector3.one);
        }
    }
}
