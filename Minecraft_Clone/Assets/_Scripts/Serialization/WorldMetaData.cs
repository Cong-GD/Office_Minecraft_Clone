using CongTDev.Collection;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace Minecraft.Serialization
{
    [Serializable]
    public class WorldMetaData
    {
        public string name = string.Empty;
        public string seed = string.Empty;
        public DateTime firstPlayTime;
        public DateTime lastPlayTime;
        public GameMode gameMode;
        public Texture2D icon;

        public WorldMetaData()
        {
        }

        public WorldMetaData Clone()
        {
            WorldMetaData clone = new();
            clone.name = name;
            clone.seed = seed;
            clone.firstPlayTime = firstPlayTime;
            clone.lastPlayTime = lastPlayTime;
            clone.gameMode = gameMode;
            clone.icon = icon;
            return clone;
        }

        public WorldMetaData(ByteString byteString)
        {
            ByteString.BytesReader byteReader = byteString.GetBytesReader();
            seed = byteReader.ReadChars().ToString();
            firstPlayTime = byteReader.ReadValue<DateTime>();
            lastPlayTime = byteReader.ReadValue<DateTime>();
            gameMode = byteReader.ReadValue<GameMode>();
        }

        public ByteString ToByteString()
        {
            ByteString byteString = ByteString.Create(200);
            byteString.WriteChars(seed);
            byteString.WriteValue(firstPlayTime);
            byteString.WriteValue(lastPlayTime);
            byteString.WriteValue(gameMode);
            return byteString;
        }
    }
}
