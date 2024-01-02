using CongTDev.Collection;
using Cysharp.Threading.Tasks;
using Minecraft.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
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

        public string WorldName => _currentWorld.name;

        public GameMode GameMode => _currentWorld.gameMode;

        public Camera MainCamera => _mainCamera ? _mainCamera : _mainCamera = Camera.main;

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
            await SceneManager.LoadSceneAsync("VoxelWorld");
            Debug.Log($"Enter voxel world name: {WorldName}");
            LoadWorldData();
        }

        public async UniTaskVoid SaveAndReturnToMainMenu()
        {
            await SaveWorldDatasAsync();
            await SceneManager.LoadSceneAsync("MainMenu");
        }

        public void LoadWorldData()
        {
            string path = Path.Combine(FileHandler.PersistentDataPath, WorldName, "Datas");
            Dictionary<string, ByteString> byteDatas = FileHandler.LoadByteDatas(path);
            OnGameLoad?.Invoke(byteDatas);
            Debug.Log($"Loaded world data: {WorldName}");
        }

        public void SaveWorldData()
        {
            Debug.Log($"Save world data: {WorldName}");
            Dictionary<string, ByteString> byteDatas = new Dictionary<string, ByteString>();
            OnGameSave?.Invoke(byteDatas);
            string path = Path.Combine(FileHandler.PersistentDataPath, WorldName, "Datas");
            FileHandler.SaveByteDatas(path, byteDatas, true);
        }

        public async UniTask LoadWorldDatasAsync()
        {
            Debug.Log($"Load world data: {WorldName}");
            string path = Path.Combine(FileHandler.PersistentDataPath, WorldName, "Datas");
            Dictionary<string, ByteString> byteDatas = 
                await UniTask.RunOnThreadPool(() => FileHandler.LoadByteDatas(path));
            OnGameLoad?.Invoke(byteDatas);
        }

        public async UniTask SaveWorldDatasAsync()
        {
            Debug.Log($"Save world data: {WorldName}");
            Dictionary<string, ByteString> byteDatas = new Dictionary<string, ByteString>();
            OnGameSave?.Invoke(byteDatas);
            string path = Path.Combine(FileHandler.PersistentDataPath, WorldName, "Datas");
            await UniTask.RunOnThreadPool(() => FileHandler.SaveByteDatas(path, byteDatas, true));
            if(MainCamera.TryGetComponent(out CameraCapture cameraCapture))
            {
                _currentWorld.icon = cameraCapture.Capture();
            }
            FileHandler.SaveWorldData(_currentWorld);
        }

    }
        
}
