using CongTDev.Collection;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Minecraft.Serialization
{
    public static class FileHandler
    {
        private static object _lock = new object();

        public const string DATE_TIME_FORMAT = "dd/MM/yyyy - hh:mm tt";

        public const string META_FILE_NAME = "meta.dat";
        public const string ICON_FILE_NAME = "icon.png";
        public static readonly string PersistentDataPath = Application.persistentDataPath;

        public static async UniTask<List<WorldMetaData>> GetWorldMetaDatasAsync(bool configureAwait = true)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PersistentDataPath);
            await UniTask.SwitchToThreadPool();
            List<WorldMetaData> worldMetaDatas = new List<WorldMetaData>();
            foreach (DirectoryInfo directory in directoryInfo.EnumerateDirectories())
            {
                try
                {
                    using FileStream fileStream = new FileStream(Path.Combine(directory.FullName, META_FILE_NAME), FileMode.Open, FileAccess.Read);
                    using ByteString byteString = ByteString.Create(fileStream);
                    WorldMetaData worldMetaData = new(byteString)
                    {
                        name = directory.Name,
                    };
                    string iconPath = Path.Combine(directory.FullName, ICON_FILE_NAME);
                    if(File.Exists(iconPath))
                    {
                        byte[] textureData = File.ReadAllBytes(iconPath);
                        await UniTask.SwitchToMainThread();
                        worldMetaData.icon = new Texture2D(0, 0);
                        worldMetaData.icon.LoadImage(textureData);
                        await UniTask.SwitchToThreadPool();
                    }
                    worldMetaDatas.Add(worldMetaData);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to load world meta data from {directory.FullName}: {e}");
                }
            }
            worldMetaDatas.Sort((a, b) => b.lastPlayTime.CompareTo(a.lastPlayTime));
            if(configureAwait)
            {
                await UniTask.SwitchToMainThread();
            }

            return worldMetaDatas;
        }

        public static bool WorldExists(string worldName)
        {
            return Directory.Exists(Path.Combine(PersistentDataPath, worldName));
        }

        public static WorldMetaData CreateNewWorld(string worldName, string seed, GameMode gameMode)
        {
            string worldDirectory = Path.Combine(PersistentDataPath, worldName);
            Directory.CreateDirectory(worldDirectory);

            string metaFilePath = Path.Combine(worldDirectory, META_FILE_NAME);
            WorldMetaData world = new WorldMetaData
            {
                name = worldName,
                seed = seed,
                firstPlayTime = DateTime.Now,
                lastPlayTime = DateTime.Now,
                gameMode = gameMode,
            };
            using ByteString byteString = world.ToByteString();
            SaveByteData(metaFilePath, byteString);
            return world;
        }

        public static void SaveWorldData(WorldMetaData world)
        {
            string worldDirectory = Path.Combine(PersistentDataPath, world.name);
            string metaFilePath = Path.Combine(worldDirectory, META_FILE_NAME);
            using ByteString byteString = world.ToByteString();
            SaveByteData(metaFilePath, byteString);
            if(world.icon != null)
            {
                string iconPath = Path.Combine(worldDirectory, ICON_FILE_NAME);
                SaveTexture2D(iconPath, world.icon);
            }
        }

        public static void DeleteWorld(string worldName)
        {
            string worldDirectory = Path.Combine(PersistentDataPath, worldName);
            Directory.Delete(worldDirectory, true);
        }

        public static void SaveByteData(string path, ByteString byteData)
        {
            if(byteData == null)
            {
                throw new ArgumentNullException(nameof(byteData));
            }
            lock (_lock)
            {
                using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                fileStream.Write(byteData.AsSpan());
            }
        }

        public static ByteString LoadByteData(string path)
        {
            lock (_lock)
            {
                using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                ByteString byteData = ByteString.Create(fileStream);
                return byteData;
            }
        }

        public static void SaveByteDatas(string directory, Dictionary<string, ByteString> byteDatas, bool disposeAfterWrite = false)
        {
            lock (_lock)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                foreach ((string fileName, ByteString byteData) in byteDatas)
                {
                    string path = Path.Combine(directory, fileName);
                    try
                    {
                        SaveByteData(path, byteData);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Error when save file {path}: {e}");
                    }
                    finally
                    {
                        if (disposeAfterWrite)
                        {
                            byteData.Dispose();
                        }
                    }
                }
            }
        }

        public static Dictionary<string, ByteString> LoadByteDatas(string directory)
        {
            Dictionary<string, ByteString> byteDatas = new Dictionary<string, ByteString>();
            lock (_lock)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                if (directoryInfo.Exists)
                {
                    foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles())
                    {
                        try
                        {
                            ByteString byteString = LoadByteData(fileInfo.FullName);
                            byteDatas.Add(fileInfo.Name, byteString);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Error when load file {fileInfo.FullName}: {e}");
                        }
                    }
                }
            }
            return byteDatas;
        }

        public static void SaveTexture2D(string path, Texture2D texture)
        {
            try
            {
                lock (_lock)
                {
                    using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
                    byte[] bytes = texture.EncodeToPNG();
                    fileStream.Write(bytes);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Fail to save texture at path {path}: {e}");
            }
        }

        public static Texture2D LoadTexture2D(string path)
        {
            try
            {
                lock (_lock)
                {
                    using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    Texture2D texture = new Texture2D(0, 0);
                    byte[] bytes = new byte[fileStream.Length];
                    fileStream.Read(bytes);
                    texture.LoadImage(bytes);
                    return texture;
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load texture from {path}: {e.Message}");
                return null;
            }
        }
    }
}
