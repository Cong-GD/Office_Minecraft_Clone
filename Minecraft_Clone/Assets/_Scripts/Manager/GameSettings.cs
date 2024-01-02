using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace Minecraft
{
    public class GameSettings
    {
        private static GameSettings _instance;
        public static GameSettings Instance => _instance ??= new GameSettings();

        private int _fieldOfView;
        private int _renderDistance;
        private int _maxFrameRate;
        private bool _isFullScreen;
        private int _mouseSensitivity;
        private int _shadowQuality;

        private GameSettings()
        {
            _fieldOfView = PlayerPrefs.GetInt("fieldOfView", 70);
            _renderDistance = PlayerPrefs.GetInt("renderDistance", 8);
            _maxFrameRate = PlayerPrefs.GetInt("maxFrameRate", 120);
            _isFullScreen = PlayerPrefs.GetInt("isFullScreen", 1) == 1;
            _mouseSensitivity = PlayerPrefs.GetInt("mouseSensitivity", 100);
            _shadowQuality = PlayerPrefs.GetInt("shadowQuality", (int)QualitySettings.shadows);
        }

        ~GameSettings()
        {
            PlayerPrefs.SetFloat("fieldOfView", _fieldOfView);
            PlayerPrefs.SetInt("renderDistance", _renderDistance);
            PlayerPrefs.SetInt("maxFrameRate", _maxFrameRate);
            PlayerPrefs.SetInt("isFullScreen", _isFullScreen ? 1 : 0);
            PlayerPrefs.SetFloat("mouseSensitivity", _mouseSensitivity);
            PlayerPrefs.SetInt("shadowQuality", _shadowQuality);
        }

        public int FieldOfView
        {
            get => _fieldOfView;
            set
            {
                _fieldOfView = value;

            }
        }

        public int RenderDistance
        {
            get => _renderDistance;
            set
            {
                value = Mathf.Clamp(value, 2, 32);
                _renderDistance = value;
            }
        }

        public int MaxFrameRate
        {
            get => _maxFrameRate;
            set
            {
                value = Mathf.Max(value, 10);
                _maxFrameRate = value;
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                _isFullScreen = value;
            }
        }

        public int MouseSensitivity
        {
            get => _mouseSensitivity;
            set
            {
                value = Mathf.Clamp(value, 0, 200);
                _mouseSensitivity = value;
            }
        }

        public ShadowQuality ShadowQuality
        {
            get => (ShadowQuality)_shadowQuality;
            set
            {
                _shadowQuality = (int)value;
            }
        }

        public void Apply()
        {
            GameManager.Instance.MainCamera.fieldOfView = _fieldOfView;
            Application.targetFrameRate = _maxFrameRate;
            Screen.fullScreen = _isFullScreen;
            QualitySettings.shadows = (ShadowQuality)_shadowQuality;
            Cinemachine.CinemachineVirtualCamera virtureCam = GameManager.Instance.MainCamera.GetComponent<Cinemachine.CinemachineBrain>()?.ActiveVirtualCamera
                as Cinemachine.CinemachineVirtualCamera;
            if (virtureCam != null)
            {
                virtureCam.m_Lens.FarClipPlane = _renderDistance * 16;
            }

        }

    }
}