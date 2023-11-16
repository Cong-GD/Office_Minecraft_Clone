using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class UIBlastFurnace : MonoBehaviour
{
    [SerializeField]
    private UIItemSlot burnSlot;

    [SerializeField]
    private UIItemSlot cookSlot;

    [SerializeField]
    private ResultUIItemSlot resultSlot;

    private BlastFurnace _furnace;

    [SerializeField]
    private RectTransform burnProgressBar;

    [SerializeField]
    private RectTransform smeltProgressBar;

    [SerializeField]
    private Image burnProgressImage;

    [SerializeField]
    private Image smeltProgressImage;

    public float BurnProgress => _furnace != null ? _furnace.BurnProgressValue : 0f;

    public float SmeltProgress => _furnace != null ? _furnace.SmeltProgressValue : 0f;

    private void OnEnable()
    {
        TransformHelper.CopyRectTransform(burnProgressBar, burnProgressImage.rectTransform);
        TransformHelper.CopyRectTransform(smeltProgressBar, smeltProgressImage.rectTransform);
        burnProgressImage.gameObject.SetActive(true);
        smeltProgressImage.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        ClearFurnace();
        burnProgressImage.gameObject.SetActive(false);
        smeltProgressImage.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        burnProgressImage.fillAmount = BurnProgress;
        smeltProgressImage.fillAmount = SmeltProgress;
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