using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIToolBar : MonoBehaviour
{
    [SerializeField] private Inventory inventory;
    [SerializeField] private UIItemSlot[] uiItemSlots;
    [SerializeField] private Transform selectionImage;

    private int _currentSelected = 0;

    private void Start()
    {
        for (int i = 0; i < inventory.toolBarItems.Length && i < uiItemSlots.Length; i++)
        {
            uiItemSlots[i].SetSlot(inventory.toolBarItems[i]);
        }
        UpdateSelectedUI();
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0f)
            return;

        _currentSelected = (int)Mathf.Repeat(_currentSelected + Mathf.Sign(scroll), uiItemSlots.Length);
        UpdateSelectedUI();

    }

    private void UpdateSelectedUI()
    {
        selectionImage.position = uiItemSlots[_currentSelected].transform.position;
        inventory.SetHand(uiItemSlots[_currentSelected].Slot);
    }

}
