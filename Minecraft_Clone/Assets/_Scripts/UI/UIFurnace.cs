using Minecraft;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class UIFurnace : MonoBehaviour
{
    [SerializeField]
    private UIItemSlot burnSlot;

    [SerializeField]
    private UIItemSlot smeltSlot;

    [SerializeField]
    private ResultUIItemSlot resultSlot;

    [SerializeField]
    private ProgressDisplayer burnProgress;

    [SerializeField]
    private ProgressDisplayer smeltProgress;

    [SerializeField]
    private Canvas myCanvas;

    public float BurnProgress => _furnace != null ? _furnace.BurnProgressValue : 0f;

    public float SmeltProgress => _furnace != null ? _furnace.SmeltProgressValue : 0f;

    private Furnace _furnace;

    private void OnEnable()
    {
        burnProgress.Enable();
        smeltProgress.Enable();
        myCanvas.enabled = true;
    }

    private void OnDisable()
    {
        ClearFurnace();
        burnProgress.Disable();
        smeltProgress.Disable();
        myCanvas.enabled = false;
    }

    private void FixedUpdate()
    {
        burnProgress.SetValue(BurnProgress);
        smeltProgress.SetValue(SmeltProgress);
    }

    public void SetFurnace(Furnace furnace)
    {
        ClearFurnace();
        if (furnace == null)
            return;

        _furnace = furnace;
        burnSlot.SetSlot(_furnace.burnSlot);
        smeltSlot.SetSlot(_furnace.smeltSlot);
        resultSlot.SetResultGiver(_furnace);
    }


    public void ClearFurnace()
    {
        if (_furnace == null)
            return;

        _furnace = null;
        burnSlot.ClearSlot();
        smeltSlot.ClearSlot();
        resultSlot.SetResultGiver(null);
    }

}