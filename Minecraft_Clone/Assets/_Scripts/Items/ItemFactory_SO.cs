using UnityEngine;

public abstract class ItemFactory_SO : ScriptableObject
{
    public abstract ItemSlot Create();
}
