using CongTDev.Collection;

public static class BlockStateSerializeHelper
{
    public enum BlockStateID : byte
    {
        Null = 0,
        Furnace = 1,
    }

    public static void GetSerializedData(ByteString byteString, IBlockState blockState)
    {
        if (blockState == null)
        {
            byteString.WriteValue(BlockStateID.Null);
            return;
        }

        if (blockState is Furnace furnace)
        {
            byteString.WriteValue(BlockStateID.Furnace);
            furnace.GetSerializedData(byteString);
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
        }
        return blockState;
    }

}