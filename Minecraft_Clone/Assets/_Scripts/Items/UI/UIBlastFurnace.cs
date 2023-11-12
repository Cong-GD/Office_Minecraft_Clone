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

    [ProgressBar("Cook Progress", 1f, EColor.Blue)]
    public float cookProgress;

    public float BurnProgress => _furnace != null ? _furnace.BurnProgressValue : 0f;

    public float CookProgress => _furnace != null ? _furnace.CookProgressValue : 0f;

    private void OnDisable()
    {
        ClearFurnace();
    }

    private void Update()
    {
        burnProgress = BurnProgress;
        cookProgress = CookProgress;
    }

    public void SetFurnace(BlastFurnace furnace)
    {
        ClearFurnace();
        if (furnace == null)
            return;

        _furnace = furnace;
        burnSlot.SetSlot(_furnace.burnSlot);
        cookSlot.SetSlot(_furnace.cookSlot);
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