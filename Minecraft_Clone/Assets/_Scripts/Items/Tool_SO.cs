
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/Tool")]
public class Tool_SO : FunctionlessItem_SO, ITool
{
    [field: SerializeField]
    [field: BoxGroup("As Tool")]
    public ToolTier ToolTier { get; private set; }

    [field: SerializeField]
    [field: BoxGroup("As Tool")]
    public ToolType ToolType { get; private set; }
}