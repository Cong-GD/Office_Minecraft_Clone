using System.Collections.Generic;
using UnityEngine;

public interface IStructure
{
    public string Name { get; }
    public IEnumerable<ModifierUnit> GetStructure();
}

public abstract class Structure : ScriptableObject
{
    public abstract IEnumerable<ModifierUnit> GetModifications(Vector3Int position);
}