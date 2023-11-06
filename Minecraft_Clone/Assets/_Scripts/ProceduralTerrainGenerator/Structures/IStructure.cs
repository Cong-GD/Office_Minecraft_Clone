using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.ProceduralTerrain.Structures
{
    public interface IStructure
    {
        public string Name { get; }
        public void GetModifications(Queue<ModifierUnit> modifiers, Vector3Int position);
    }
}
