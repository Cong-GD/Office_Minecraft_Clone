using CongTDev.Collection;
using Minecraft;

public static class BlockStateSerializeHelper
{
    public enum BlockStateID : byte
    {
        Null = 0,
        Furnace = 1,
        Stogare = 2,
    }

    public static void GetSerializedData(ByteString byteString, IBlockState blockState)
    {
        if (blockState == null)
        {
            byteString.WriteValue(BlockStateID.Null);
            return;
        }
        else if (blockState is Furnace furnace)
        {
            byteString.WriteValue(BlockStateID.Furnace);
            furnace.GetSerializedData(byteString);
        }
        else if (blockState is Stogare storage)
        {
            byteString.WriteValue(BlockStateID.Stogare);
            storage.GetSerializedData(byteString);
        }
    }

    public static IBlockState GetBlockState(ref ByteString.BytesReader byteReader)
    {
        BlockStateID blockStateType = byteReader.ReadValue<BlockStateID>();
        if (blockStateType == BlockStateID.Null)
            return null;

        IBlockState blockState = null;
        switch (blockStateType)
        {
            case BlockStateID.Furnace:
                blockState = new Furnace(ref byteReader);
                break;
            case BlockStateID.Stogare:
                blockState = new Stogare(ref byteReader);
                break;
        }
        return blockState;
    }

}