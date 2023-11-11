using NaughtyAttributes;
using UnityEngine;

public class CookAbleBlock_SO : BlockData_SO, ICookAbleItem
{
    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public float CookDuration { get; private set;}

    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public ItemPacked CookResult { get; private set; }
}
