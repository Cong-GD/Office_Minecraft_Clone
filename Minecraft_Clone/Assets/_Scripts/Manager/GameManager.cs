using CongTDev.Collection;
using Cysharp.Threading.Tasks;
using Minecraft.Input;
using Minecraft.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Minecraft
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField]
        private CameraCapture cameraCapture;


        public event Action<Dictionary<string, ByteString>> OnGameSave;

        public event Action<Dictionary<string, ByteString>> OnGameLoad;

        public event Action<GameMode> OnGameModeChange;

        public string WorldName => _currentWorld.name;

        public Camera MainCamera => _mainCamera ? _mainCamera : _mainCamera = Camera.main;

        public GameMode GameMode
        {
            get => _currentWorld.gameMode;
            set
            {
                _currentWorld.gameMode = value;
                OnGameModeChange?.Invoke(value);
            }
        }

        public Statictics Statictics { get; private set; } = new Statictics();

        private WorldMetaData _currentWorld = new WorldMetaData();

        private Camera _mainCamera;


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
        }

        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Instance = Instantiate(Resources.Load<GameManager>("GameManager"), Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(Instance.gameObject);
        }

        public async UniTaskVoid LoadWorld(WorldMetaData metaWorldData)
        {
            _currentWorld = metaWorldData;
            WorldSettings.WorldSeed = metaWorldData.seed.GetHashCode();

            string staticticsPath = Path.Combine(FileHandler.PersistentDataPath, WorldName, "Statictics.dat");
            if(File.Exists(staticticsPath))
            {
                using ByteString staticticsData = FileHandler.LoadByteData(staticticsPath);
                Statictics.Load(staticticsData);
            }
            else
            {
                Statictics = new Statictics();
            }
           
            Statictics.totalPlayCount++;
            string path = Path.Combine(FileHandler.PersistentDataPath, WorldName, "World");
            Dictionary<string, ByteString> byteDatas = await UniTask.RunOnThreadPool(() => FileHandler.LoadByteDatas(path));
            await SceneManager.LoadSceneAsync("VoxelWorld");
            MInput.state = MInput.State.Gameplay;
            OnGameLoad?.Invoke(byteDatas);
        }

        public async UniTaskVoid SaveAndReturnToMainMenu()
        {
            await SaveWorldDatasAsync();
            await SceneManager.LoadSceneAsync("MainMenu");
            MInput.state = MInput.State.None;
        }


        public async UniTask SaveWorldDatasAsync()
        {
            Debug.Log($"Save world data: {WorldName}");
            Dictionary<string, ByteString> byteDatas = new Dictionary<string, ByteString>();
            OnGameSave?.Invoke(byteDatas);
            string path = Path.Combine(FileHandler.PersistentDataPath, WorldName, "World");
            await UniTask.RunOnThreadPool(() => FileHandler.SaveByteDatas(path, byteDatas, true));

            if (MainCamera.TryGetComponent(out CameraCapture cameraCapture))
            {
                _currentWorld.icon = cameraCapture.Capture();
            }
            _currentWorld.firstPlayTime = DateTime.Now;

            using(ByteString byteString = Statictics.ToByteString())
            {
                string staticticsPath = Path.Combine(FileHandler.PersistentDataPath, WorldName, "Statictics.dat");
                FileHandler.SaveByteData(staticticsPath, byteString);
            }

            FileHandler.SaveWorldData(_currentWorld);
        }

    }

}
