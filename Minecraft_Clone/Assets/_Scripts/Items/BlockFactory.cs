using UnityEngine;

[CreateAssetMenu(menuName = "Item/Factory/Block")]
public class BlockFactory : ItemFactory
{
    [SerializeField] private BlockData blockToCreate;

    [Min(1)]
    [SerializeField] private int amount;

    public override ItemSlot Create()
    {
        ItemSlot slot = new ItemSlot();
        slot.SetItem(blockToCreate, amount);
        return slot;
    }
}