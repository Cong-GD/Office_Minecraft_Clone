using CongTDev.Collection;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.ProceduralTerrain.Structures
{
    public abstract class Structure_SO : ScriptableObject, IStructure
    {
        public string Name => name;

        public abstract void GetModifications(MyNativeList<ModifierUnit> modifiers, Vector3Int position);

    }
}