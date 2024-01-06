using UnityEngine;

namespace Minecraft
{
    public enum ArmorType
    {
        Helmet,
        Chestplate,
        Leggings,
        Boots
    }

    [CreateAssetMenu(menuName = "Minecraft/Item/Armor")]
    public class Armor_SO : FunctionlessItem_SO
    {
        [field: SerializeField]
        public ArmorType ArmorType { get; private set; }

        [field: SerializeField]
        public int ArmorPoint { get; private set; }

        [field: SerializeField]
        public int Toughness { get; private set; }
    }
}
