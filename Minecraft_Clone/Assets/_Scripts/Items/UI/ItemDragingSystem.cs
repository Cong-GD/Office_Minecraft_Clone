using Minecraft.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragingSystem : MonoBehaviour
{
    private List<RaycastResult> _rayCastResut = new();

    [SerializeField] private UIItemSlot draggingSlot;
    private bool _isDragging;

    private void Awake()
    {
        draggingSlot.Slot = new ItemSlot();
    }

    private void Update()
    {
        if (MInput.UI_LeftClick.WasPerformedThisFrame())
        {
            if(GetCurrentMouseOverSlot(out var slot))
            {
                UIItemSlot_OnSlotLeftClick(slot);
            }
        }

        if(_isDragging)
        {
            draggingSlot.transform.position = (Vector2)Input.mousePosition;
        }
    }

    private bool GetCurrentMouseOverSlot(out UIItemSlot slot)
    {
        RaycastUtilities.UIRaycast((Vector2)Input.mousePosition, _rayCastResut);
        foreach (var hit in _rayCastResut)
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
        if (!_isDragging)
        {
            HandlerStartDragItemSlot(uiItemSlot);
        }
        else
        {
            HandlerEndDragItemSlot(uiItemSlot);
        }
    }

    private void HandlerEndDragItemSlot(UIItemSlot uiItemSlot)
    {
        if (!uiItemSlot.HasItem())
        {
            draggingSlot.Slot.TransferTo(uiItemSlot.Slot, draggingSlot.Slot.Amount);
            _isDragging = false;
        }
        else
        {
            draggingSlot.Slot.SwapItem(uiItemSlot.Slot);
        }
    }

    private void HandlerStartDragItemSlot(UIItemSlot uiItemSlot)
    {
        if (!uiItemSlot.HasItem())
            return;

        _isDragging = true;
        uiItemSlot.Slot.TransferTo(draggingSlot.Slot, uiItemSlot.Slot.Amount);
    }

    private void UIItemSlot_OnSlotRightClick(UIItemSlot uiItemSlot)
    {

    }
}
