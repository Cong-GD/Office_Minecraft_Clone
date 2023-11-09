using Minecraft.Input;
using UnityEngine;

public class UIToolBar : MonoBehaviour
{
    [SerializeField] private UIItemSlot[] uiItemSlots;
    [SerializeField] private Transform selectionImage;

    private int _currentSelected = 0;

    private void Start()
    {
        var inventorySystem = InventorySystem.Instance;
        for (int i = 0; i < inventorySystem.toolBarItems.Length && i < uiItemSlots.Length; i++)
        {
            uiItemSlots[i].SetSlot(inventorySystem.toolBarItems[i]);
        }
        UpdateSelectedUI();
        MInput.ScrollWheel.performed += OnMouseWheelScroll;
    }

    private void OnDestroy()
    {
        MInput.ScrollWheel.performed -= OnMouseWheelScroll;
    }

    private void OnMouseWheelScroll(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        float scroll = context.ReadValue<float>();

        _currentSelected = (int)Mathf.Repeat(_currentSelected + Mathf.Sign(scroll), uiItemSlots.Length);
        UpdateSelectedUI();
    }

    private void UpdateSelectedUI()
    {
        selectionImage.position = uiItemSlots[_currentSelected].transform.position;
        InventorySystem.Instance.SetRightHand(uiItemSlots[_currentSelected].Slot);
    }

}
