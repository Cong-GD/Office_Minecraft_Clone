using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Item/Factory/Block")]
public class BlockFactory : ItemFactory_SO
{
    [SerializeField] private BlockData_SO blockToCreate;

    [Min(1)]
    [SerializeField] private int amount;

    public override ItemPacked Create()
    {
        return new ItemPacked(blockToCreate, amount);
    }
}