using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.ProceduralTerrain.Structures
{
    public abstract class Structure : ScriptableObject, IStructure
    {
        public string Name => name;

        public abstract void GetModifications(Queue<ModifierUnit> modifiers, Vector3Int position);

    }
}