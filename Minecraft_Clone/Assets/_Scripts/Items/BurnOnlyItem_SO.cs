using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/BurnOnlyItem")]
public class BurnOnlyItem_SO : FunctionlessItem_SO, IBurnAbleItem
{
    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public float BurnDuration { get; private set; }
}