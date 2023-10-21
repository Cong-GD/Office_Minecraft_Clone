using UnityEngine;

public abstract class ItemFactory : ScriptableObject
{
    public abstract ItemSlot Create();
}
