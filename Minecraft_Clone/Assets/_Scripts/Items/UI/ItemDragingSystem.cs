using Minecraft.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ItemDragingSystem : MonoBehaviour
{

    [SerializeField] private UIItemSlot uiDraggingSlot;

    private List<RaycastResult> _rayCastResut = new();
    public bool IsDragging => !DraggingSlot.IsEmpty();

    public ItemSlot DraggingSlot { get; private set; } = new ItemSlot();

    private void Awake()
    {
        uiDraggingSlot.Slot = DraggingSlot;
    }

    private void OnEnable()
    {
        MInput.UI_LeftClick.performed += ProcessLeftClickInput;
        MInput.UI_RightClick.performed += ProcessRightClickInput;
    }


    private void OnDisable()
    {
        MInput.UI_LeftClick.performed -= ProcessLeftClickInput;
        MInput.UI_RightClick.performed -= ProcessRightClickInput;
    }

    private void Update()
    {
        UpdateDraggingSlotPosition();
    }

    private void UpdateDraggingSlotPosition()
    {
        if (IsDragging)
        {
            uiDraggingSlot.transform.position = MInput.PointerPosition;
        }
    }

    private void ProcessLeftClickInput(InputAction.CallbackContext context)
    {
        if (GetCurrentMouseOverSlot(out var slot))
        {
            UIItemSlot_OnSlotLeftClick(slot);
        }
    }

    private void ProcessRightClickInput(InputAction.CallbackContext context)
    {
        if (GetCurrentMouseOverSlot(out var slot))
        {
            UIItemSlot_OnSlotRightClick(slot);
        }
    }

    private bool GetCurrentMouseOverSlot(out UIItemSlot slot)
    {
        RaycastUtilities.UIRaycast(MInput.PointerPosition, _rayCastResut);
        foreach (RaycastResult hit in _rayCastResut)
        {
            if(hit.gameObject.TryGetComponent(out slot))
            {
                return true;
            }
        }
        slot = null;
        return false;
    }

    private void UIItemSlot_OnSlotLeftClick(UIItemSlot uiItemSlot)
    {
        if (uiItemSlot.Slot is null)
            return;

        if (!IsDragging)
        {
            HandlerStartDragItemSlot(uiItemSlot);
        }
        else
        {
            HandlerEndDragItemSlot(uiItemSlot);
        }
    }

    private void HandlerStartDragItemSlot(UIItemSlot uiItemSlot)
    {
        if (!uiItemSlot.HasItem())
            return;

        uiItemSlot.Slot.TryTransferTo(uiDraggingSlot.Slot, uiItemSlot.Slot.Amount);
    }
    private void HandlerEndDragItemSlot(UIItemSlot uiItemSlot)
    {
        if (DraggingSlot.TryTransferTo(uiItemSlot.Slot, DraggingSlot.Amount))
            return;

        DraggingSlot.SwapItem(uiItemSlot.Slot);
    }

    private void UIItemSlot_OnSlotRightClick(UIItemSlot uiItemSlot)
    {
        if (uiItemSlot.Slot is null)
            return;

        if (IsDragging)
        {
            DraggingSlot.TryTransferTo(uiItemSlot.Slot, 1);
        }
        else
        {
            uiItemSlot.Slot.TryTransferTo(DraggingSlot, uiItemSlot.Slot.Amount / 2);
        }
    }

}
