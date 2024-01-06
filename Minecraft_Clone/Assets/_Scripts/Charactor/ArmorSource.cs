using FMODUnity;
using System;
using UnityEngine;

namespace Minecraft
{
    public class ArmorSource
    {
        public event Action OnValueChanged;

        private ItemSlot _armorSlot;

        public ArmorSource(ItemSlot armorSlot)
        {
            _armorSlot = armorSlot;
            armorSlot.OnItemModified += OnItemModified;
        }
        ~ArmorSource()
        {
            _armorSlot.OnItemModified -= OnItemModified;
            _armorSlot = null;
        }

        public int ArmorPoint => _armorSlot.RootItem is Armor_SO armor ? armor.ArmorPoint : 0;

        public int Toughness => _armorSlot.RootItem is Armor_SO armor ? armor.Toughness : 0;

        public virtual void DamageArmor(int damage)
        {
            // Decrease armor durability
        }

        private void OnItemModified()
        {
            OnValueChanged?.Invoke();
        }

    }
}