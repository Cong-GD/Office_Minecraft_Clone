using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/CookOnlyItem")]
public class CookOnlyItem_SO : FunctionlessItem_SO, ICookAbleItem
{
    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public float CookDuration { get; private set; }

    [field: BoxGroup("Cook And Burn")]
    [field: SerializeField]
    public ItemPacked CookResult { get; private set; }
}
