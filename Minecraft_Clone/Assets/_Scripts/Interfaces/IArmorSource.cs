namespace Minecraft
{
    public interface IArmorSource
    {
        int ArmorPoint { get; }
        int Toughness { get; }
        void DamageArmor(int damage);
    }
}