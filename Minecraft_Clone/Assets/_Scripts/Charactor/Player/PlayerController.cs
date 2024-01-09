using CongTDev.Collection;
using Cysharp.Threading.Tasks;
using Minecraft.Assets._Scripts.Charactor.Player;
using Minecraft.Input;
using Minecraft.Serialization;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minecraft
{
    public class PlayerController : GlobalReference<PlayerController>
    {
        public const string PLAYER_DATA_FILE_NAME = "PlayerData.dat";

        private const int MILISECOND_PER_TICK = 50;

        [SerializeField]
        private PlayerData_SO playerData;

        [SerializeField]
        private FirstPersonController firstPersonController;

        [SerializeField]
        public PlayerFlyHandler playerFlyHandler;

        [SerializeField]
        private PlayerHUB playerHUB;

        [SerializeField]
        private PlayerCamera playerCamera;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Health health;

        [SerializeField]
        private Vector3 startSpawnOffset;

        [SerializeField]
        private float blendSpeedChangeRate;

        public bool IsFirstTimeSpawn { get; private set; } = true;

        private Vector3 _loadedPosition;
        private float _blendSpeedValue;

        protected override void Awake()
        {
            base.Awake();
            playerData.ClearTempData();
            MInput.state = MInput.State.Gameplay;
            GameManager.Instance.OnGameLoad += LoadPlayerData;
            GameManager.Instance.OnGameSave += SavePlayerData;
            MInput.Fly.performed += ProcessFlyInput;
            MInput.Sprint.performed += ProcessSprintInput;
            MInput.Crounch.performed += ProcessCronchInput;
            MInput.Crounch.canceled += ProcessCronchInput;
            World.Instance.OnWorldLoaded += SpawnPlayer;
            MInput.InputActions.GamePlay.SaveCheckPoint.performed += SaveCheckPoint;
            SlowUpdateLoop().Forget();
        }

        private void OnDestroy()
        {
            GameManager.Instance.OnGameLoad -= LoadPlayerData;
            GameManager.Instance.OnGameSave -= SavePlayerData;
            MInput.Fly.performed -= ProcessFlyInput;
            MInput.Sprint.performed -= ProcessSprintInput;
            MInput.Crounch.performed -= ProcessCronchInput;
            MInput.Crounch.canceled -= ProcessCronchInput;
            MInput.InputActions.GamePlay.SaveCheckPoint.performed -= SaveCheckPoint;
            if(World.Instance)
            {
                World.Instance.OnWorldLoaded -= SpawnPlayer;
            }
        }

        private void Start()
        {
            IsFirstTimeSpawn = GameManager.Instance.Statictics.totalPlayCount < 2;
            if (IsFirstTimeSpawn)
            {
                _loadedPosition = startSpawnOffset;
                health.MaxHealth = playerData.DefaultMaxHealth;
                health.FullHeal();

                playerData.PlayerBody.position = _loadedPosition;
            }

            GameManager.Instance.GameMode = GameMode.Creative;
        }

        private void Update()
        {
            BlendAnimation();
        }

        private async UniTaskVoid SlowUpdateLoop()
        {
            CancellationToken token = destroyCancellationToken;
            uint tick = 0;
            while(!token.IsCancellationRequested)
            {
                await UniTask.Delay(MILISECOND_PER_TICK, cancellationToken: token);
                tick++;
                UpdateOxyGen(tick);
                UpdateFood(tick);
            }
        }

        private void SavePlayerData(Dictionary<string, ByteString> byteDatas)
        {
            ByteString byteData = ByteString.Create(100);

            byteData.WriteValue(playerData.PlayerBody.position);
            byteData.WriteValue(playerData.checkPoint);
            byteData.WriteValue(playerCamera.XRotation);
            byteData.WriteValue(playerCamera.YRotation);
            byteData.WriteValue(playerData.currentFood);
            byteData.WriteValue(health.CurrentHealth);

            byteDatas[PLAYER_DATA_FILE_NAME] = byteData;
        }

        private void LoadPlayerData(Dictionary<string, ByteString> byteDatas)
        {
            try
            {
                if (byteDatas.Remove(PLAYER_DATA_FILE_NAME, out ByteString byteData))
                {
                    ByteString.BytesReader byteReader = byteData.GetBytesReader();
                    _loadedPosition = byteReader.ReadValue<Vector3>();
                    playerData.checkPoint = byteReader.ReadValue<Vector3>();
                    playerCamera.XRotation = byteReader.ReadValue<float>();
                    playerCamera.YRotation = byteReader.ReadValue<float>();
                    playerData.currentFood = byteReader.ReadValue<int>();
                    health.CurrentHealth = byteReader.ReadValue<int>();

                    playerData.PlayerBody.position = _loadedPosition;
                    playerHUB.ShowFood(playerData.MaxFood, playerData.currentFood, false);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SpawnPlayer()
        {
            if (!IsFirstTimeSpawn)
            {
                playerData.PlayerBody.position = _loadedPosition;
                return;
            }

            if (Physics.Raycast(new Vector3(startSpawnOffset.x, 250, startSpawnOffset.z), Vector3.down,
                out RaycastHit hit, 250, playerData.GroundLayer))
            {
                playerData.PlayerBody.position = hit.point.Add(y: startSpawnOffset.y);
                playerData.PlayerBody.velocity = Vector3.zero;
                playerData.checkPoint = playerData.PlayerBody.position;
            }
        }

        public void Revive()
        {
            health.Revive();
            health.FullHeal();
            playerData.currentFood = playerData.MaxFood;
        }

        public void AddFood(int amount)
        {
            playerData.currentFood = math.min(playerData.MaxFood, playerData.currentFood + amount);
            playerHUB.ShowFood(playerData.MaxFood, playerData.currentFood, false);
        }

        private void ProcessFlyInput(InputAction.CallbackContext context)
        {
            if(GameManager.Instance.GameMode != GameMode.Creative)
            {
                return;
            }

            playerData.isFlying = !playerData.isFlying;
            if (playerData.isFlying)
            {
                EnterFlyMode();
            }
            else
            {
                ExitFlyMode();
            }
        }

        private void ProcessSprintInput(InputAction.CallbackContext context)
        {
            if (playerData.isCrounching)
                return;

            playerData.isSprinting = true;
        }

        private void ProcessCronchInput(InputAction.CallbackContext context)
        {
            playerData.isCrounching = context.performed;
            if (playerData.isCrounching)
            {
                playerData.isSprinting = false;
            }
        }

        private void SaveCheckPoint(InputAction.CallbackContext context)
        {
            playerData.checkPoint = playerData.PlayerBody.position;
            health.Kill();
        }

        public void EnterFlyMode()
        {
            playerData.isFlying = true;
            firstPersonController.enabled = false;
            playerFlyHandler.enabled = true;
        }

        public void ExitFlyMode()
        {
            playerData.isFlying = false;
            playerFlyHandler.enabled = false;
            firstPersonController.enabled = true;
        }

        private void BlendAnimation()
        {
            _blendSpeedValue = math.lerp(_blendSpeedValue, playerData.currentMoveSpeed, blendSpeedChangeRate * Time.deltaTime);
            animator.SetFloat(AnimID.Speed, _blendSpeedValue);
        }

        private uint SecondToTick(float second)
        {
            return math.max(1u, (uint)(second * (1000 / MILISECOND_PER_TICK)));
        }

        private void UpdateOxyGen(uint tick)
        {
            uint oxyConsumeTick = SecondToTick(playerData.OxyConsumedTime);
            if (tick % oxyConsumeTick != 0) //|| GameManager.Instance.GameMode is GameMode.Creative)
                return;

            ref int currentOxyGen = ref playerData.currentOxygen;
            if (playerData.isHeadInWater)
            {
                if (currentOxyGen > 0)
                {
                    currentOxyGen = math.max(0, currentOxyGen - 1);
                    playerHUB.ShowOxyGen(playerData.MaxOxygen, currentOxyGen);

                }
                else
                {
                    health.TakeDamage(1, DamegeType.Drowning);
                }
            }
            else
            {
                if(currentOxyGen == playerData.MaxOxygen)
                    return;

                if (currentOxyGen % 2 != 0)
                    currentOxyGen--;

                currentOxyGen = math.min(playerData.MaxOxygen, currentOxyGen += 2);
                playerHUB.ShowOxyGen(playerData.MaxOxygen, currentOxyGen);
            }
        }

        private void UpdateFood(uint tick)
        {
            float foodComsumeTime = playerData.isSprinting ? playerData.FoodConsumedTime * 0.5f : playerData.FoodConsumedTime;
            uint foodConsumeTick = SecondToTick(foodComsumeTime);
            ref int currentFood = ref playerData.currentFood;

            if(tick % foodConsumeTick == 0)
            {
                if (currentFood > 0)
                {
                    currentFood = math.max(0, currentFood - 1);
                    playerHUB.ShowFood(playerData.MaxFood, currentFood, false);
                }

                if(tick % 15 == 0 && currentFood > 13)
                {
                    currentFood = math.max(0, currentFood - 1);
                    health.Heal(1);
                }
            }

            if(currentFood == 0 && tick % 10 == 0)
            {
                health.TakeDamage(1, DamegeType.Starving);
            }
        }
    }
}