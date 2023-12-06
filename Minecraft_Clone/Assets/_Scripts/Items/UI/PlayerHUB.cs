using Minecraft.Input;
using ObjectPooling;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUB : MonoBehaviour
{
    [SerializeField] private UIItemSlot[] uiToolBarSlots;
    [SerializeField] private Transform selectionImage;

    private int _currentSelected = 0;

    [SerializeField]
    private List<HealthPoint> healthPoints;

    [SerializeField]
    private List<HealthDisplayer> _healthDisplayers = new();

    [SerializeField]
    private ObjectPool healthDisplayerPool;

    [SerializeField]
    private Transform healthBarParent;

    [SerializeField]
    private VerticalLayoutGroup layoutGroup;

    private void FixedUpdate()
    {
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        bool isDirty = false;
        while(_healthDisplayers.Count > healthPoints.Count)
        {
            int last = _healthDisplayers.Count - 1;
            _healthDisplayers[last].ReturnToPool();
            _healthDisplayers.RemoveAt(last);
            isDirty = true;
        }

        while(_healthDisplayers.Count < healthPoints.Count)
        {
            var displayer = (HealthDisplayer)healthDisplayerPool.Get();
            displayer.transform.SetParent(healthBarParent);
            _healthDisplayers.Add(displayer);
            isDirty = true;
        }

        for (int i = 0; i < _healthDisplayers.Count; i++)
        {
            if(_healthDisplayers[i].DisplayHealth(healthPoints[i]))
                isDirty = true;
        }

        if(isDirty)
            LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.transform as RectTransform);
    }

    private void Start()
    {
        var inventorySystem = InventorySystem.Instance;
        for (int i = 0; i < inventorySystem.toolBarItems.Length && i < uiToolBarSlots.Length; i++)
        {
            uiToolBarSlots[i].SetSlot(inventorySystem.toolBarItems[i]);
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

        _currentSelected = (int)Mathf.Repeat(_currentSelected + Mathf.Sign(scroll), uiToolBarSlots.Length);
        UpdateSelectedUI();
    }

    private void UpdateSelectedUI()
    {
        selectionImage.position = uiToolBarSlots[_currentSelected].transform.position;
        InventorySystem.Instance.SetRightHand(uiToolBarSlots[_currentSelected].Slot);
    }
}
