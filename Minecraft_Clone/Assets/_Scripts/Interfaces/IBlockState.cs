using CongTDev.Collection;
using UnityEngine;

public interface IBlockState
{
    public Vector3Int Position { get; }
    public bool ValidateBlockState();
}
