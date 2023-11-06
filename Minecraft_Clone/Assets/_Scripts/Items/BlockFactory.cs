using UnityEngine;

[CreateAssetMenu(menuName = "Item/Factory/Block")]
public class BlockFactory : ItemFactory_SO
{
    [SerializeField] private BlockData_SO blockToCreate;

    [Min(1)]
    [SerializeField] private int amount;

    public override ItemSlot Create()
    {
        ItemSlot slot = new ItemSlot();
        slot.SetItem(blockToCreate, amount);
        return slot;
    }
}