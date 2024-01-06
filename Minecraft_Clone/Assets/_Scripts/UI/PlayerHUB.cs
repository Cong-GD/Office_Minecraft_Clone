using Minecraft;
using Minecraft.Input;
using ObjectPooling;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUB : MonoBehaviour
{
    [SerializeField] 
    private UIItemSlot[] uiToolBarSlots;

    [SerializeField] 
    private Transform selectionImage;

    [SerializeField]
    private Health playerHealth;

    [SerializeField]
    private Armor playerArmor;


    [SerializeField]
    private List<StatDisplayer> healthDisplayers = new();

    [SerializeField]
    private List<StatDisplayer> oxygenDisplayers = new();

    [SerializeField]
    private List<StatDisplayer> foodDisplayers = new();

    [SerializeField]
    private List<StatDisplayer> armorDisplayer = new();

    [SerializeField]
    private ObjectPool statDisplayerPool;

    [SerializeField]
    private RectTransform healthBarParent;

    [SerializeField]
    private RectTransform oxygenBarParent;

    [SerializeField]
    private RectTransform foodBarParent;

    [SerializeField]
    private RectTransform armorBarParent;

    [SerializeField]
    private ProgressDisplayer experimentDisplayer;

    private int _currentSelected = 0;

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
        playerArmor.OnValueChanged.AddListener(UpdateArmorPoint);
        UpdateArmorPoint();
    }

    private void OnDestroy()
    {
        MInput.ScrollWheel.performed -= OnMouseWheelScroll;
        playerHealth.OnValueChanged.RemoveListener(UpdateHealthBar);
        playerArmor.OnValueChanged.RemoveListener(UpdateArmorPoint);
    }

    public void UpdateHealthBar()
    {
        int healthPointCount = (playerHealth.MaxHealth + 1) / 2;
        int absorptionPointCount = (playerHealth.AbsorptionAmount + 1) / 2;
        
        ValidateLength(healthDisplayers, healthPointCount + absorptionPointCount, healthBarParent);

        for (int i = 0; i < healthPointCount; i++)
        {
            StatPoint healthPoint = new StatPoint
            {
                amount = GetAmountValue(playerHealth.CurrentHealth, (i + 1) * 2),
                state = StatPoint.State.Health
            };
            healthDisplayers[i].SetStat(healthPoint);
        }

        for (int i = 0; i < absorptionPointCount; i++)
        {
            StatPoint healthPoint = new StatPoint
            {
                amount = GetAmountValue(playerHealth.AbsorptionAmountRemaining, (i + 1) * 2),
                state = StatPoint.State.AbsorbingHealth
            };
            healthDisplayers[i + healthPointCount].SetStat(healthPoint);
        }
    }

    public void UpdateArmorPoint()
    {
        ShowArmor(playerArmor.MaxArmorPoint, playerArmor.ArmorPoint);
    }

    public void ShowOxyGen(int maxValue ,int currentValue)
    {
        ValidateLength(oxygenDisplayers, (maxValue + 1) / 2, oxygenBarParent);

        if (currentValue == maxValue)
        {
            currentValue = 0;
        }

        for (int i = 0; i < oxygenDisplayers.Count; i++)
        {
            StatPoint oxygenPoint = new StatPoint 
            {
                amount = GetAmountValue(currentValue, (i + 1) * 2),
                state = StatPoint.State.Oxygen
            };
            oxygenDisplayers[i].SetStat(oxygenPoint);
        }
    }

    public void ShowFood(int maxValue, int currentValue, bool isSaturation)
    {
        ValidateLength(foodDisplayers, (maxValue + 1) / 2, foodBarParent);

        StatPoint.State foodState = isSaturation ? StatPoint.State.Saturation : StatPoint.State.Food;

        for (int i = 0; i < foodDisplayers.Count; i++)
        {
            StatPoint foodPoint = new StatPoint
            {
                amount = GetAmountValue(currentValue, (i + 1) * 2),
                state = foodState
            };
            foodDisplayers[i].SetStat(foodPoint);
        }
    }

    public void ShowArmor(int maxValue, int currentValue) 
    {
        ValidateLength(armorDisplayer, (maxValue + 1) / 2, armorBarParent);

        for (int i = 0; i < armorDisplayer.Count; i++)
        {
            StatPoint armorPoint = new StatPoint
            {
                amount = GetAmountValue(currentValue, (i + 1) * 2),
                state = StatPoint.State.Armor
            };
            armorDisplayer[i].SetStat(armorPoint);
        }
    }

    private void ValidateLength(List<StatDisplayer> displayers, int length, RectTransform parent)
    {
        bool isDirty = false;

        while(displayers.Count > length)
        {
            int last = displayers.Count - 1;
            displayers[last].ReturnToPool();
            displayers.RemoveAt(last);
            isDirty = true;
        }

        while(displayers.Count < length)
        {
            StatDisplayer displayer = (StatDisplayer)statDisplayerPool.Get(parent);
            displayers.Add(displayer);
            isDirty = true;
        }

        if (isDirty)
        {
            LayoutRebuilder.MarkLayoutForRebuild(parent);
        }
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

    private StatPoint.Amount GetAmountValue(int currentValue, int position)
    {
        int distance = 2 - (position - currentValue);
        distance = Mathf.Clamp(distance, 0, 2);
        return (StatPoint.Amount)distance;
    }

}
