using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : GlobalReference<UIManager>
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private UIItemSlot draggingSLot;

    private bool _isActive;
    private bool _isDragging;


    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (value == _isActive)
                return;

            _isActive = value;

            playerInventory.gameObject.SetActive(value);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        draggingSLot.Slot = new ItemSlot();

        UIItemSlot.OnSlotLeftClick += UIItemSlot_OnSlotLeftClick;
        UIItemSlot.OnSlotRightClick += UIItemSlot_OnSlotRightClick;
    }

    private void OnDestroy()
    {
        UIItemSlot.OnSlotLeftClick -= UIItemSlot_OnSlotLeftClick;
        UIItemSlot.OnSlotRightClick -= UIItemSlot_OnSlotRightClick;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Debug.Log(RaycastUtilities.PointerIsOverUI((Vector2)Input.mousePosition).Count);
        }

        if (!_isDragging)
            return;

        draggingSLot.transform.position = (Vector2)Input.mousePosition;
    }

    private void UIItemSlot_OnSlotLeftClick(UIItemSlot uiItemSlot)
    {
        if(!_isDragging)
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
        if(!uiItemSlot.HasItem())
        {
            draggingSLot.Slot.TransferTo(uiItemSlot.Slot, draggingSLot.Slot.Amount);
            _isDragging = false;
        }
    }

    private void HandlerStartDragItemSlot(UIItemSlot uiItemSlot)
    {
        if (!uiItemSlot.HasItem())
            return;

        _isDragging = true;
        uiItemSlot.Slot.TransferTo(draggingSLot.Slot, uiItemSlot.Slot.Amount);
    }

    private void UIItemSlot_OnSlotRightClick(UIItemSlot uiItemSlot)
    {
        
    }
}


public static class RaycastUtilities
{
    public static List<RaycastResult> PointerIsOverUI(Vector2 screenPos)
    {
        var hitObject = UIRaycast(ScreenPosToPointerData(screenPos));
        return hitObject;
    }

    static List<RaycastResult> UIRaycast(PointerEventData pointerData)
    {
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results;
    }

    static PointerEventData ScreenPosToPointerData(Vector2 screenPos)
       => new(EventSystem.current) { position = screenPos };
}