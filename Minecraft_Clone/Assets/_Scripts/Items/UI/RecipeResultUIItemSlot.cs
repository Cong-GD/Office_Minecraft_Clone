using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RecipeResultUIItemSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private ItemSlot _displaySlot;
    private ManufactureSpace _manufactureSpace;

    private ItemSlot _resultSlot = new();

    public bool HasItem() => _displaySlot != null && !_displaySlot.IsEmpty();

    private void Start()
    {
        SetDisplaySlot(new ItemSlot());
    }

    private void OnDestroy()
    {
        SetManufactureSpace(null);
    }

    public void SetManufactureSpace(ManufactureSpace manufactureSpace)
    {
        if (_manufactureSpace != null)
        {
            _manufactureSpace.OnCheckedResult -= OnManufactureResultChecked;
        }

        if(manufactureSpace != null)
        {
            manufactureSpace.OnCheckedResult += OnManufactureResultChecked;
        }
        _manufactureSpace = manufactureSpace;
    }

    private void OnManufactureResultChecked(ItemPacked itemPacked)
    {
        _displaySlot.SetItem(itemPacked);
    }

    private void SetDisplaySlot(ItemSlot slot)
    {
        _displaySlot = slot;
        if (_displaySlot != null)
        {
            _displaySlot.OnItemModified += UpdateUI;
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (!HasItem())
        {
            iconImage.enabled = false;
            amountText.enabled = false;
            return;
        }

        iconImage.enabled = true;
        amountText.enabled = _displaySlot.Amount < 2 ? false : true;
        iconImage.sprite = _displaySlot.RootItem.Icon;
        amountText.text = _displaySlot.Amount.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(_manufactureSpace == null) 
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        var draggingSystem = InventorySystem.Instance.DragingSystem;
        _resultSlot.SetItem(_manufactureSpace.TakeResult());
        if (_resultSlot.IsEmpty())
            return;
        
        _resultSlot.TryTransferTo(draggingSystem.DraggingSlot, _resultSlot.Amount);
        if (!_resultSlot.IsEmpty())
            InventorySystem.Instance.AddItemToInventory(_resultSlot);

    }
}