using Minecraft;
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
    private List<HealthDisplayer> _healthDisplayers = new();

    [SerializeField]
    private Health playerHealth;

    [SerializeField]
    private ObjectPool healthDisplayerPool;

    [SerializeField]
    private Transform healthBarParent;

    [SerializeField]
    private VerticalLayoutGroup layoutGroup;
    private void Start()
    {
        InventorySystem inventorySystem = InventorySystem.Instance;
        for (int i = 0; i < inventorySystem.toolBarItems.Length && i < uiToolBarSlots.Length; i++)
        {
            uiToolBarSlots[i].SetSlot(inventorySystem.toolBarItems[i]);
        }
        UpdateSelectedUI();
        MInput.ScrollWheel.performed += OnMouseWheelScroll;
        playerHealth.OnValueChanged.AddListener(UpdateHealthBar);
        UpdateHealthBar();
    }

    private void OnDestroy()
    {
        MInput.ScrollWheel.performed -= OnMouseWheelScroll;
        playerHealth.OnValueChanged.RemoveListener(UpdateHealthBar);
    }

    public void UpdateHealthBar()
    {
        bool isDirty = false;
        int healthPointCount = (playerHealth.MaxHealth + 1) / 2;
        int absorptionPointCount = (playerHealth.AbsorptionAmount + 1) / 2;
        int totalHealthCount = healthPointCount + absorptionPointCount;
        
        while(_healthDisplayers.Count > totalHealthCount)
        {
            int last = _healthDisplayers.Count - 1;
            _healthDisplayers[last].ReturnToPool();
            _healthDisplayers.RemoveAt(last);
            isDirty = true;
        }

        while(_healthDisplayers.Count < totalHealthCount)
        {
            HealthDisplayer displayer = (HealthDisplayer)healthDisplayerPool.Get();
            displayer.transform.SetParent(healthBarParent);
            _healthDisplayers.Add(displayer);
            isDirty = true;
        }

        for (int i = 0; i < healthPointCount; i++)
        {
            int amount = (i + 1) * 2;
            HealthPoint healthPoint = new HealthPoint();
            int distance = 2 - (amount - playerHealth.CurrentHealth);
            distance = Mathf.Clamp(distance, 0, 2);
            healthPoint.amount = (HealthPoint.Amount)distance;
            _healthDisplayers[i].DisplayHealth(healthPoint);
        }

        for(int i = 0; i < absorptionPointCount; i++)
        {
            int amount = (i + 1) * 2;
            HealthPoint healthPoint = new HealthPoint();
            int distance = 2 - (amount - playerHealth.AbsorptionAmountRemaining);
            distance = Mathf.Clamp(distance, 0, 2);
            healthPoint.amount = (HealthPoint.Amount)distance;
            healthPoint.state = HealthPoint.State.Absorbing;
            _healthDisplayers[i + healthPointCount].DisplayHealth(healthPoint);
        }

        if(isDirty)
            LayoutRebuilder.MarkLayoutForRebuild(layoutGroup.transform as RectTransform);
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
