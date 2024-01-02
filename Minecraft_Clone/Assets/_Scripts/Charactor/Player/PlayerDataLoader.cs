using CongTDev.Collection;
using Minecraft.Serialization;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Minecraft
{
    public class PlayerDataLoader : MonoBehaviour
    {
        public const string FILE_NAME = "PlayerData.dat";

        public PlayerData_SO playerData;

        public bool IsPlayerDataLoaded { get; private set; }


        [SerializeField]
        private Vector3 spawnOffset;

        private Vector3 playerPosition;

        private void Awake()
        {
            GameManager.Instance.OnGameLoad += LoadPlayerData;
            GameManager.Instance.OnGameSave += SavePlayerData;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnGameLoad -= LoadPlayerData;
            GameManager.Instance.OnGameSave -= SavePlayerData;
        }

        private void LoadPlayerData(Dictionary<string, ByteString> byteDatas)
        {
            if(byteDatas.TryGetValue(FILE_NAME, out ByteString byteString))
            {
                ByteString.BytesReader byteReader = byteString.GetBytesReader();
                playerPosition = byteReader.ReadValue<Vector3>();
                IsPlayerDataLoaded = true;
            }
            else
            {
                playerPosition = spawnOffset;
                IsPlayerDataLoaded = false;
            }

            playerData.PlayerBody.position = playerPosition;
        }

        private void SavePlayerData(Dictionary<string, ByteString> byteDatas)
        {
            ByteString byteString = new ByteString();
            byteString.WriteValue(playerData.PlayerBody.position);
            byteDatas[FILE_NAME] = byteString;
        }

        public void SpawnPlayer()
        {
            if (IsPlayerDataLoaded)
            {
                playerData.PlayerBody.position = this.playerPosition;
                return;
            }

            Vector3 playerPosition = spawnOffset;
            if (Physics.Raycast(new Vector3(playerPosition.x, 250, playerPosition.z), Vector3.down,
                out RaycastHit hit, 250, LayerMask.GetMask("Ground")))
            {
                playerData.PlayerBody.position = hit.point.Add(y: spawnOffset.y);
                playerData.PlayerBody.velocity = Vector3.zero;
            }
        }



        //Test
        [Button]
        private unsafe void TestSave()
        {
            using ByteString byteString = ByteString.Create(12);
            byteString.WriteValue(spawnOffset);
            FileHandler.SaveByteData("Assets/test.txt", byteString);

        }

        [Button]
        private void TestLoad()
        {
            using ByteString byteString = FileHandler.LoadByteData("Assets/test.txt");
            ByteString.BytesReader byteReader = byteString.GetBytesReader();
            spawnOffset = byteReader.ReadValue<Vector3>();
        }

    }
}
