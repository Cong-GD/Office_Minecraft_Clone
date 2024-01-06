using CongTDev.Collection;
using System;

namespace Minecraft
{
    public class Statictics
    {
        public const string FILE_NAME = "Statictics.dat";
        public int totalPlayCount;
        public int totalDeath;
        public int totalKill;
        

        public void Load(ByteString byteString)
        {
            ByteString.BytesReader byteReader = byteString.GetBytesReader();
            totalPlayCount = byteReader.ReadValue<int>();
            totalDeath = byteReader.ReadValue<int>();
            totalKill = byteReader.ReadValue<int>();
        }

        public ByteString ToByteString()
        {
            ByteString byteString = ByteString.Create(16);
            byteString.WriteValue(totalPlayCount);
            byteString.WriteValue(totalDeath);
            byteString.WriteValue(totalKill);
            return byteString;
        }
    }

}
