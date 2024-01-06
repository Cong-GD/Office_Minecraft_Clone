using Minecraft;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultUIItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private TextMeshProUGUI amountText;

    private ItemSlot _displaySlot = new();
    private IResultGiver _resultGiver;
    private ItemSlot _resultSlot = new();

    public bool HasItem() => !_displaySlot.IsEmpty();

    private void Awake()
    {
        _displaySlot.OnItemModified += UpdateUI;
    }

    private void OnDestroy()
    {
        SetResultGiver(null);
    }

    public void SetResultGiver(IResultGiver resultGiver)
    {
        if (_resultGiver != null)
        {
            _resultGiver.OnCheckedResult -= OnManufactureResultChecked;
            _displaySlot.SetItem(ItemPacked.Empty);
        }

        if (resultGiver != null)
        {
            resultGiver.OnCheckedResult += OnManufactureResultChecked;
            _displaySlot.SetItem(resultGiver.PeekResult());
        }
        _resultGiver = resultGiver;
    }

    private void OnManufactureResultChecked(ItemPacked itemPacked)
    {
        _displaySlot.SetItem(itemPacked);
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
        if (_resultGiver == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        var draggingSystem = UIManager.Instance.DraggingSystem;
        _resultSlot.SetItem(_resultGiver.TakeResult());
        if (_resultSlot.IsEmpty())
            return;

        _resultSlot.TryTransferTo(draggingSystem.DraggingSlot, _resultSlot.Amount);
        if (!_resultSlot.IsEmpty())
            InventorySystem.Instance.AddItemToInventory(_resultSlot);

        _displaySlot.SetItem(_resultGiver.PeekResult());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.Instance.HideToolTip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HasItem())
            ToolTip.Instance.ShowToolTip(_displaySlot.RootItem.GetTooltipText(),
                Minecraft.Input.MInput.PointerPosition,
                5f);
    }
}
