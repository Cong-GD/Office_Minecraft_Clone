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
        public DateTime creationTime;
        public GameMode gameMode;
        public Texture2D icon;

        public WorldMetaData()
        {
        }

        public WorldMetaData Clone()
        {
            return new WorldMetaData
            {
                name = name,
                seed = seed,
                creationTime = creationTime,
                gameMode = gameMode,
                icon = icon
            };
        }

        public WorldMetaData(ByteString byteString)
        {
            ByteString.BytesReader byteReader = byteString.GetBytesReader();
            seed = byteReader.ReadChars().ToString();
            creationTime = byteReader.ReadValue<DateTime>();
            gameMode = byteReader.ReadValue<GameMode>();
        }

        public ByteString ToByteString()
        {
            ByteString byteString = ByteString.Create(200);
            byteString.WriteChars(seed);
            byteString.WriteValue(creationTime);
            byteString.WriteValue(gameMode);
            return byteString;
        }
    }
}
