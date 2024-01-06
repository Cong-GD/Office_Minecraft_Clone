using Minecraft;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CreavityUIItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image iconImage;

    [field: SerializeField]
    public UnityEvent<PointerEventData, CreavityUIItemSlot> OnClick { get; private set; }
    public BaseItem_SO Item
    {
        get => _item;
        set => SetItem(value);
    }

    private BaseItem_SO _item;

    public void SetItem(BaseItem_SO item)
    {
        _item = item;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_item == null)
        {
            iconImage.enabled = false;
        }
        else
        {
            iconImage.enabled = true;
            iconImage.sprite = _item.Icon;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick.Invoke(eventData, this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item != null)
            ToolTip.Instance.ShowToolTip(_item.GetTooltipText(),
                Minecraft.Input.MInput.PointerPosition,
                5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTip.Instance.HideToolTip();
    }
}