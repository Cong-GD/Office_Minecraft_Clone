using NaughtyAttributes;
using UnityEngine;

public class UIBlastFurnace : MonoBehaviour
{
    [SerializeField]
    private UIItemSlot burnSlot;

    [SerializeField]
    private UIItemSlot cookSlot;

    [SerializeField]
    private ResultUIItemSlot resultSlot;

    private BlastFurnace _furnace;

    [ProgressBar("Burn Progress", 1f, EColor.Red)]
    public float burnProgress;

    [ProgressBar("Smelt Progress", 1f, EColor.Blue)]
    public float smeltProgress;

    public float BurnProgress => _furnace != null ? _furnace.BurnProgressValue : 0f;

    public float SmeltProgress => _furnace != null ? _furnace.SmeltProgressValue : 0f;

    private void OnDisable()
    {
        ClearFurnace();
    }

    private void Update()
    {
        burnProgress = BurnProgress;
        smeltProgress = SmeltProgress;
    }

    public void SetFurnace(BlastFurnace furnace)
    {
        ClearFurnace();
        if (furnace == null)
            return;

        _furnace = furnace;
        burnSlot.SetSlot(_furnace.burnSlot);
        cookSlot.SetSlot(_furnace.smeltSlot);
        resultSlot.SetResultGiver(_furnace);
    }


    public void ClearFurnace()
    {
        if (_furnace == null)
            return;

        _furnace = null;
        burnSlot.ClearSlot();
        cookSlot.ClearSlot();
        resultSlot.SetResultGiver(null);
    }

}