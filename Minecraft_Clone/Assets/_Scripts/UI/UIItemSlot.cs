﻿using Minecraft;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private ItemSlot _slot;
    public ItemSlot Slot
    {
        get => _slot; 
        set => SetSlot(value);
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void OnDestroy()
    {
        SetSlot(null);
    }

    public bool HasItem() => _slot != null && !_slot.IsEmpty();

    public void SetSlot(ItemSlot slot)
    {
        ClearSlot();
        _slot = slot;
        if(slot != null)
        {
            slot.OnItemModified += UpdateUI;
            UpdateUI();
        }
       
    }

    public void ClearSlot()
    {
        if (_slot == null)
            return;

        _slot.OnItemModified -= UpdateUI;
        _slot = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (!isActiveAndEnabled)
            return;

        if (!HasItem())
        {
            iconImage.enabled = false;
            amountText.enabled = false;
            return;
        }

        iconImage.enabled = true;
        amountText.enabled = _slot.Amount < 2 ? false : true;
        iconImage.sprite = _slot.RootItem.Icon;
        amountText.text = _slot.Amount.ToString();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.Instance.HideToolTip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HasItem())
            ToolTip.Instance.ShowToolTip(_slot.RootItem.GetTooltipText(), 
                Minecraft.Input.MInput.PointerPosition, 
                5f);
    }
}
