namespace Minecraft
{
    public class ArmorRequiment : IItemSlotRequiment
    {
        public ArmorType ArmorType { get; }

        public ArmorRequiment(ArmorType armorType)
        {
            ArmorType = armorType;
        }

        public bool CheckRequiment(BaseItem_SO item)
        {
            return item is Armor_SO armor && armor.ArmorType == ArmorType;
        }
    }
}