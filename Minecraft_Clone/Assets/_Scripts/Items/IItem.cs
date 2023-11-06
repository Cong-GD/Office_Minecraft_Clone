using Minecraft.ProceduralMeshGenerate;
using System;
using UnityEngine;

public interface IItem
{
    public string Name { get; }
    public Sprite Icon { get; }
    public int MaxStack { get; }
    public ObjectMeshData ObjectMeshData { get; }

}
