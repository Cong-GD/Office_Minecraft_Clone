using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private ItemSlot _slot;
    public ItemSlot Slot
    {
        get => _slot; 
        set => SetSlot(value);
    }

    public bool HasItem() => _slot != null && !_slot.IsEmpty();

    public void SetSlot(ItemSlot slot)
    {
        ClearSlot();
        slot.OnItemChanged += UpdateUI;
        _slot = slot;
        UpdateUI();
    }

    public void ClearSlot()
    {
        if (_slot == null)
            return;

        _slot.OnItemChanged -= UpdateUI;
        _slot = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (_slot == null || _slot.Item == null)
        {
            iconImage.enabled = false;
            amountText.enabled = false;
            return;
        }
        iconImage.enabled = true;
        amountText.enabled = true;
        iconImage.sprite = _slot.Item.Icon;
        amountText.text = _slot.Amount.ToString();
    }
}