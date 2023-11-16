using Minecraft;
using Minecraft.Input;
using NaughtyAttributes;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Collections.LowLevel.Unsafe;


public class PlayerInteract : MonoBehaviour
{
    [SerializeField, Required]
    private Transform eye;

    [Min(1f)]
    public float checkDistance;

    public float dropForce = 1f;

    [SerializeField]
    private int destroyInputReceiveDelayInMilisecond;

    [SerializeField]
    private LayerMask entityLayer;

    [SerializeField]
    private LayerMask groundLayer;

    private bool isCastHit;

    private Vector3Int hitPosition;
    private Vector3Int adjacentHitPosition;

    private readonly Vector3 _halfOne = new Vector3(0.5f, 0.5f, 0.5f);

    private RepeatingCancellationTokenSource _destroyCancelationTokenSource = new();

    private void OnEnable()
    {
        MInput.Destroy.performed += OnLeftClicked;
        MInput.Destroy.canceled += OnLeftClicked;
        MInput.Build.performed += OnRightClicked;
        MInput.Throw.performed += ProcessThrowInput;
    }

    private void OnDisable()
    {
        MInput.Destroy.performed -= OnLeftClicked;
        MInput.Destroy.canceled -= OnLeftClicked;
        MInput.Build.performed -= OnRightClicked;
        MInput.Throw.performed -= ProcessThrowInput;
    }

    private void OnDestroy()
    {
        _destroyCancelationTokenSource.Destroy();
    }

    private void OnRightClicked(InputAction.CallbackContext context)
    {
        RayCast();

        if (!isCastHit || HasInteractedWithBlock())
            return;

        CheckForPlaceBlock();
    }

    private void OnLeftClicked(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            try
            {
                CheckForDestroy();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("End destroying blocks");
            }

        }
        else
        {
            _destroyCancelationTokenSource.Reset();
        }
    }

    private void CheckForPlaceBlock()
    {
        if (!IsSafeForPlaceBlock(adjacentHitPosition))
            return;

        if (!World.Instance.CanEdit())
            return;

        var rightHand = InventorySystem.Instance.RightHand;
        if (ItemSlot.IsNullOrEmpty(rightHand))
            return;

        if (rightHand.RootItem is not BlockData_SO blockData)
            return;

        rightHand.TakeAmount(1);
        var direction = GetDirectionWithPlayer(hitPosition + _halfOne);
        var _ = World.Instance.EditBlockAsync(adjacentHitPosition, blockData.BlockType, direction);
    }

    private async void CheckForDestroy()
    {
        
        RayCast();
        if (!isCastHit)
            return;

        var block = Chunk.GetBlock(hitPosition).Data();
        if (block.BlockType == BlockType.Air)
            return;

        var isSucceed = await World.Instance.EditBlockAsync(hitPosition, BlockType.Air, Direction.Backward);
        if (isSucceed)
        {
            PickupManager.Instance.ThrowItem(new(block, 1), hitPosition + _halfOne, (Vector3.up + UnityEngine.Random.insideUnitSphere) * dropForce);
        }
    }

    private Direction GetDirectionWithPlayer(Vector3 pos)
    {
        var direction = Vector2Int.RoundToInt((eye.position - pos).XZ().normalized);

        if (direction.x == 0)
            return direction.y >= 0 ? Direction.Forward : Direction.Backward;

        return direction.x >= 0 ? Direction.Right : Direction.Left;
    }

    private void ProcessThrowInput(InputAction.CallbackContext obj)
    {
        if (ItemSlot.IsNullOrEmpty(InventorySystem.Instance.RightHand))
            return;

        var itemPacked = InventorySystem.Instance.RightHand.TakeAmount(1);
        PickupManager.Instance.ThrowItem(itemPacked, eye.position + eye.forward, eye.forward * 1.5f);
    }

    private bool HasInteractedWithBlock()
    {
        if (MInput.Shift.IsPressed())
            return false;

        var blockHit = Chunk.GetBlock(hitPosition).Data();

        if (blockHit is IInteractable interactable)
        {
            interactable.Interact(hitPosition);
            return true;
        }
        return false;
    }


    private void RayCast()
    {
        var ray = new Ray(eye.transform.position, eye.transform.forward);

        if (Physics.Raycast(ray, out var hit, checkDistance, groundLayer))
        {
            hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal * 0.5f);
            adjacentHitPosition = Vector3Int.FloorToInt(hit.point + hit.normal * 0.5f);

            isCastHit = true;
        }
        else
        {
            isCastHit = false;
        }
    }

    private bool IsSafeForPlaceBlock(Vector3Int position)
    {
        return !Physics.CheckBox(position + _halfOne, _halfOne, Quaternion.identity, entityLayer);
    }

    private void OnDrawGizmosSelected()
    {
        RayCast();
        if (isCastHit)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(adjacentHitPosition + _halfOne, Vector3.one);
        }
    }
}
