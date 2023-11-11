using NaughtyAttributes;
using UnityEngine;

public class BurnAbleBlock_SO : BlockData_SO, IBurnAbleItem
{
    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public float BurnDuration { get; private set;}
}
