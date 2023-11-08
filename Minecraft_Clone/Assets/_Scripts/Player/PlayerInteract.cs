using Minecraft.Input;
using Minecraft.ProceduralMeshGenerate;
using NaughtyAttributes;
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

    [SerializeField]
    private ObjectPooling.ObjectPool freeObejctPool;

    private bool destroyFound;
    private bool placeFound;

    private Vector3Int destroyPos;
    private Vector3Int placePos;

    private readonly Vector3 _halfOne = new Vector3(0.5f, 0.5f, 0.5f);

    private void OnEnable()
    {
        MInput.LeftMouse.performed += OnLeftClicked;
        MInput.RightMouse.performed += OnRightClicked;
        MInput.Throw.performed += ThrowItem;
    }

    private void OnDisable()
    {
        MInput.LeftMouse.performed -= OnLeftClicked;
        MInput.RightMouse.performed -= OnRightClicked;
        MInput.Throw.performed -= ThrowItem;
    }

    private void OnRightClicked(InputAction.CallbackContext context)
    {
        CheckForPlaceBlock();
    }

    private void OnLeftClicked(InputAction.CallbackContext context)
    {
        CheckForDestroy();
    }

    private void CheckForDestroy()
    {
        RayCast();
        if (!destroyFound)
            return;

        // On testing
        var block = Chunk.GetBlock(destroyPos).Data();
        var freeObject = (FreeMinecraftObject)freeObejctPool.Get();
        freeObject.Init(new(block, 1), destroyPos, Vector3.zero);

        World.Instance.EditBlock(destroyPos, BlockType.Air);
    }

    private void CheckForPlaceBlock()
    {
        RayCast();
        if (!placeFound || InventorySystem.Instance.RightHand.RootItem is not BlockData_SO blockData)
            return;

        if (Physics.CheckBox(placePos + _halfOne, _halfOne, Quaternion.identity, entityLayer))
            return;

        InventorySystem.Instance.RightHand.TakeAmount(1, out _);
        World.Instance.EditBlock(placePos, blockData.BlockType);
    }

    private void ThrowItem(InputAction.CallbackContext obj)
    {
        var itemPacked = InventorySystem.Instance.RightHand.TakeAmount(1);
        if (itemPacked.IsEmpty())
            return;

        var freeObject = (FreeMinecraftObject)freeObejctPool.Get();
        freeObject.Init(itemPacked, eye.position + eye.forward, eye.forward * 1.5f);
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