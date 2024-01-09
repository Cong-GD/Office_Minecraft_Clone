using CongTDev.Collection;
using Minecraft.AI;
using ObjectPooling;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public class MonsterSpawner : MonoBehaviour
    {
        [SerializeField]
        private PlayerData_SO playerData_SO;

        [SerializeField]
        private DayNightSystem dayNightSystem;

        [SerializeField]
        private ObjectPool monsterPool;

        [SerializeField]
        private float deSpawnRadius = 100f;

        [SerializeField]
        private float spawnRadius = 50f;

        [SerializeField]
        private float spawnInterval = 4f;

        [SerializeField]
        private int maxSpawnCount = 10;

        [SerializeField]
        private LayerMask groundLayer;

        [SerializeField, Range(0f, 24f)]
        private float startOfNight = 18f;

        [SerializeField, Range(0f, 24f)]
        private float startOfDay = 6f;

        private List<BaseMonster> _spawnedMonster = new();
        private float _lastTimeSpawn;
        private ByteString _monsterData;

        private void Awake()
        {
            GameManager.Instance.OnGameSave += OnGameSave;
            GameManager.Instance.OnGameLoad += OnGameLoad;
            World.Instance.OnWorldLoaded += LoadMonsters;
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnGameSave -= OnGameSave;
            GameManager.Instance.OnGameLoad -= OnGameLoad;
            if (World.Instance)
            {
                World.Instance.OnWorldLoaded -= LoadMonsters;
            }
        }

        private void Update()
        {
            if (IsNightTime())
            {
                if (Time.time > _lastTimeSpawn + spawnInterval && _spawnedMonster.Count < maxSpawnCount)
                {
                    _lastTimeSpawn = Time.time;
                    SpawnMonster();
                }
                UpdateMonster();
            }
            else
            {
                ClearMonsters();
            }
        }

        private void OnGameSave(Dictionary<string, ByteString> dictionary)
        {
            ByteString byteString = ByteString.Create();
            byteString.WriteValue(_spawnedMonster.Count);
            foreach (BaseMonster monster in _spawnedMonster)
            {
                using ByteString monsterData = monster.ToByteString();
                byteString.WriteByteString(monsterData);
            }
            dictionary["Monster.dat"] = byteString;
        }

        private void OnGameLoad(Dictionary<string, ByteString> dictionary)
        {
            dictionary.Remove("Monster.dat", out _monsterData);
        }

        private void LoadMonsters()
        {
            if (_monsterData == null)
            {
                return;
            }
            ClearMonsters();
            ByteString.BytesReader reader = _monsterData.GetBytesReader();
            int monsterCount = reader.ReadValue<int>();
            for (int i = 0; i < monsterCount; i++)
            {
                using ByteString monsterData = reader.ReadByteString();
                BaseMonster monster = (BaseMonster)monsterPool.Get(transform);
                monster.FromByteString(monsterData);
                _spawnedMonster.Add(monster);
            }
            _monsterData.Dispose();
            _monsterData = null;
        }

        private bool IsNightTime()
        {
            float currentHour = dayNightSystem.CurrentHourInDay;
            return currentHour >= startOfNight || currentHour < startOfDay;
        }

        private void SpawnMonster()
        {
            Vector3 playerPosition = playerData_SO.PlayerBody.position;
            float spawnPosX = playerPosition.x + UnityEngine.Random.Range(-spawnRadius, spawnRadius);
            float spawnPosZ = playerPosition.z + UnityEngine.Random.Range(-spawnRadius, spawnRadius);
            if (Physics.Raycast(new Vector3(spawnPosX, WorldSettings.MAP_HEIGHT_IN_BLOCK, spawnPosZ), Vector3.down, out RaycastHit hitInfo, WorldSettings.MAP_HEIGHT_IN_BLOCK, groundLayer))
            {
                SpawnMonster(hitInfo.point);
            }
        }

        private void UpdateMonster()
        {
            for (int i = _spawnedMonster.Count - 1; i >= 0; i--)
            {
                BaseMonster monster = _spawnedMonster[i];
                if (monster.Health.IsAlive == false)
                {
                    PickupManager.Instance.ThrowItem(monster.DropItem, monster.transform.position, Vector3.zero);
                    monster.ReturnToPool();
                    _spawnedMonster.RemoveAt(i);
                    return;
                }
                else if (Vector3.Distance(monster.transform.position, playerData_SO.PlayerBody.position) > deSpawnRadius)
                {
                    monster.ReturnToPool();
                    _spawnedMonster.RemoveAt(i);
                    return;
                }
            }
        }

        private void SpawnMonster(Vector3 spawnPos)
        {
            BaseMonster monster = (BaseMonster)monsterPool.Get(transform);
            monster.Health.Revive();
            monster.Health.FullHeal();
            monster.SetPosition(spawnPos);
            _spawnedMonster.Add(monster);
        }

        private void ClearMonsters()
        {
            for (int i = _spawnedMonster.Count - 1; i >= 0; i--)
            {
                _spawnedMonster[i].ReturnToPool();
                _spawnedMonster.RemoveAt(i);
            }
        }
    }
}
