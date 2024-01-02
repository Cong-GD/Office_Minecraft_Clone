using CongTDev.Collection;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.ProceduralTerrain.Structures
{
    public interface IStructure
    {
        public string Name { get; }
        public void GetModifications(MyNativeList<ModifierUnit> modifiers, Vector3Int position);
    }
}
