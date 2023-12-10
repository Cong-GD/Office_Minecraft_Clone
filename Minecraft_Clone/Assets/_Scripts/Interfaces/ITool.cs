using Unity.VisualScripting;

public interface ITool
{
    public ToolTier ToolTier { get; }
    public ToolType ToolType { get; }
}

public static class MiningHelper
{
    public static int GetToolMultilier(this ITool tool)
    {
        return tool.ToolTier switch
        {
            ToolTier.None => 1,
            ToolTier.Wood => 2,
            ToolTier.Stone => 4,
            ToolTier.Iron => 6,
            ToolTier.Diamond => 8,
            ToolTier.Gold => 12,
            _ => 1
        };
    }

    private static readonly ITool _bareHand = new BareHand();

    public static ITool AsTool(this BaseItem_SO item)
    {
        if(item is ITool tool)
            return tool;

        return _bareHand;
    }

    public static ITool GetTool(this ItemSlot slot)
    {
        if(slot is null)
            return _bareHand;

        return slot.RootItem.AsTool();
    }
}

public class BareHand : ITool
{
    public ToolTier ToolTier => ToolTier.None;

    public ToolType ToolType => ToolType.BareHand;
}