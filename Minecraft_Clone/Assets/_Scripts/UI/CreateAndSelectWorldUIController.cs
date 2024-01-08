using CongTDev.Collection;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using FMODUnity;
using Minecraft.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace Minecraft
{
    public class CreateAndSelectWorldUIController : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField worldNameInputField;

        [SerializeField]
        private TMP_InputField seedInputField;

        [SerializeField]
        private Canvas createWorldCanvas;

        [Header("Game Mode")]
        [SerializeField]
        private GameMode gameMode;

        [SerializeField]
        private TextMeshProUGUI gameModeText;

        [SerializeField]
        private TextMeshProUGUI gameModeDescription;

        [SerializeField]
        [TextArea(3, 10)]
        private string survivalDescription;

        [SerializeField]
        [TextArea(3, 10)]
        private string creativeDescription;

        [SerializeField]
        [TextArea(3, 10)]
        private string hardcoreDescription;

        [Header("World List")]
        [SerializeField]
        private Transform worldListContent;

        [SerializeField]
        private WorldInfo worldInfoPrefab;

        [SerializeField]
        private MinecraftButton playSelectedButton;

        [SerializeField]
        private MinecraftButton deleteSelectedButton;

        [SerializeField]
        private MinecraftButton editSelectedButton;

        [SerializeField]
        private MinecraftButton reCreateSelectedButton;

        private List<WorldInfo> _worldInfoList = new List<WorldInfo>();
        private WorldInfo _selectedWorldInfo;

        private void OnValidate()
        {
            if (gameMode is not (GameMode.Survival or GameMode.Creative or GameMode.Hardcore))
            {
                gameMode = GameMode.Survival;
            }

            SetGameMode(gameMode);
        }

        public void UpdateWorldList()
        {
            UpdateWorldListAsync().Forget();
        }

        public async UniTaskVoid UpdateWorldListAsync()
        {
            foreach (WorldInfo worldInfo in _worldInfoList)
            {
                Destroy(worldInfo.gameObject);
            }
            _worldInfoList.Clear();
            foreach (WorldMetaData worldMetaData in await FileHandler.GetWorldMetaDatasAsync())
            {
                WorldInfo worldInfo = Instantiate(worldInfoPrefab, worldListContent);
                worldInfo.SetWorldInfo(worldMetaData);
                _worldInfoList.Add(worldInfo);
                worldInfo.OnClickEvent.AddListener(SetSelectedWorldInfo);
            }
        }

        public void ClearSelectedWorldInfo()
        {
            SetSelectedWorldInfo(null);
        }

        public void SetSelectedWorldInfo(WorldInfo worldInfo)
        {
            foreach (WorldInfo info in _worldInfoList)
            {
                info.SetSelected(false);
            }
            if(worldInfo)
            {
                worldInfo.SetSelected(true);
            }
            _selectedWorldInfo = worldInfo;
            UpdateButtonInteracable();
        }

        private void UpdateButtonInteracable()
        {
            bool isInteractable = _selectedWorldInfo != null;
            playSelectedButton.SetInteractable(isInteractable);
            deleteSelectedButton.SetInteractable(isInteractable);
            //editSelectedButton.SetInteractable(isInteractable);
            reCreateSelectedButton.SetInteractable(isInteractable);
        }

        public void OnPlayButtonClicked()
        {
            if (_selectedWorldInfo == null)
            {
                return;
            }

            if(!FileHandler.WorldExists(_selectedWorldInfo.WorldData.name))
            {
                ConfirmPanel.Instance.Show($"World '{_selectedWorldInfo.WorldData.name}' does not exist");
                return;
            }

            GameManager.Instance.LoadWorld(_selectedWorldInfo.WorldData).Forget();
        }

        public void OnDeleteButtonClicked()
        {
            if (_selectedWorldInfo == null)
            {
                return;
            }

            ConfirmPanel.Instance.Show(
                $"Are you sure you want to delete this world?\r\n" +
                $"'{_selectedWorldInfo.WorldData.name}' will be lost forever! (A long time!)", 
                confirmMessage: "Delete",
                onConfirm: () => {
                    FileHandler.DeleteWorld(_selectedWorldInfo.WorldData.name);
                    _worldInfoList.Remove(_selectedWorldInfo);
                    Destroy(_selectedWorldInfo.gameObject);
                    SetSelectedWorldInfo(null);
                });
        }

        public void OnEditButtonClicked()
        {
            if (_selectedWorldInfo == null)
            {
                return;
            }
            // Todo: Edit world meta data and save it
        }

        public void OnCreateNewButtonClicked()
        {
            createWorldCanvas.enabled = true;
            worldNameInputField.text = "New World";
            SetGameMode(GameMode.Survival);
            seedInputField.text = "";
        }

        public void OnReCreateButtonClicked()
        {
            if (_selectedWorldInfo == null)
            {
                return;
            }

            createWorldCanvas.enabled = true;
            worldNameInputField.text = $"Copy of {_selectedWorldInfo.WorldData.name}";
            SetGameMode(_selectedWorldInfo.WorldData.gameMode);
            seedInputField.text = _selectedWorldInfo.WorldData.seed;
        }


        public void OnGameModeButtonClicked()
        {
            switch (gameMode)
            {
                case GameMode.Survival:
                    SetGameMode(GameMode.Creative);
                    break;
                case GameMode.Creative:
                    SetGameMode(GameMode.Hardcore);
                    break;
                case GameMode.Hardcore:
                    SetGameMode(GameMode.Survival);
                    break;
                default:
                    break;
            }
        }

        public void OnCreateWorldButtonClicked()
        {
            string worldName = worldNameInputField.text;
            if (string.IsNullOrWhiteSpace(worldName))
            {
                ConfirmPanel.Instance.Show("World name cannot be empty");
                return;
            }

            string seed = seedInputField.text;
            if (string.IsNullOrWhiteSpace(seed))
            {
                seed = Guid.NewGuid().ToString();
            }

            if(FileHandler.WorldExists(worldName))
            {
                ConfirmPanel.Instance.Show($"World '{worldName}' already exists");
                return;
            }

            CreateNewWorld(worldName, seed, gameMode);
        }

        private void CreateNewWorld(string worldName, string seed, GameMode gameMode)
        {
            WorldMetaData worldData = FileHandler.CreateNewWorld(worldName, seed, gameMode);
            GameManager.Instance.LoadWorld(worldData).Forget();
        }

        public void SetGameMode(GameMode gameMode)
        {
            this.gameMode = gameMode;
            if(gameModeText)
            {
                gameModeText.SetText($"Game Mode: {gameMode}");
            }
            if(gameModeDescription)
            {
                gameModeDescription?.SetText(gameMode switch
                {
                    GameMode.Survival => survivalDescription,
                    GameMode.Creative => creativeDescription,
                    GameMode.Hardcore => hardcoreDescription,
                    _ => "Unexpected mode",
                });
            }
            
        }
    }
}
