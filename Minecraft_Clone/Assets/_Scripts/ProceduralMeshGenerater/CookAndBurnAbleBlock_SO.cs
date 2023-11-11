using NaughtyAttributes;
using UnityEngine;

public class CookAndBurnAbleBlock_SO : BlockData_SO , ICookAbleItem, IBurnAbleItem
{
    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public float BurnDuration { get; private set; }

    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public float CookDuration { get; private set; }

    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public ItemPacked CookResult { get; private set; }
}