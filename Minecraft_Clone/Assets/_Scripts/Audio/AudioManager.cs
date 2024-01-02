using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Minecraft.Audio
{
    public enum MusicName
    {
        None = 0,
        Menu = 1,
        Ingame = 2
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public const string MASTER_VOLUME = "MasterVolume";
        public const string MUSIC_VOLUME = "MusicVolume";
        public const string ENTITY_VOLUME = "EntityVolume";
        public const string ENVIROMENT_VOLUME = "EnviromentVolume";
        public const string SFX_VOLUME = "SFXVolume";
        public const string GUI_VOLUME = "GUIVolume";

        [SerializeField]
        private EventReference musicEvent;

        public bool DebugMode;

        private EventInstance _musicEventInstance;
        private PARAMETER_ID _musicNameParameterID;
        private List<EventInstance> _eventInstances;

        private Bus _masterBus;
        private Bus _musicBus;
        private Bus _entityBus;
        private Bus _enviromentBus;
        private Bus _sfxBus;
        private Bus _guiBus;

        private float _masterVolume;
        private float _musicVolume;
        private float _entityVolume;
        private float _enviromentVolume;
        private float _sfxVolume;
        private float _guiVolume;


        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Mathf.Clamp01(value);
                SetBusVolume(ref _masterBus, _masterVolume);
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                SetBusVolume(ref _musicBus, _musicVolume);
                if(IsMusicMuted)
                {
                    StopMusic();
                }
                else
                {
                    TryStartMusic();
                }
            }
        }

        public float EntityVolume
        {
            get => _entityVolume;
            set
            {
                _entityVolume = Mathf.Clamp01(value);
                SetBusVolume(ref _entityBus, _entityVolume);
            }
        }

        public float EnvironmentVolume
        {
            get => _enviromentVolume;
            set
            {
                _enviromentVolume = Mathf.Clamp01(value);
                SetBusVolume(ref _enviromentBus, _enviromentVolume);
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp01(value);
                SetBusVolume(ref _sfxBus, _sfxVolume);
            }
        }

        public float GUIVolume
        {
            get => _guiVolume;
            set
            {
                _guiVolume = Mathf.Clamp01(value);
                SetBusVolume(ref _guiBus, _guiVolume);
            }
        }

        public bool IsMusicMuted => Mathf.Approximately(MusicVolume, 0f);

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _eventInstances = new List<EventInstance>();
            InitializeBus();
            InitializeMusicEvent();

            MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME, 1f);
            MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME, 1f);
            EntityVolume = PlayerPrefs.GetFloat(ENTITY_VOLUME, 1f);
            EnvironmentVolume = PlayerPrefs.GetFloat(ENVIROMENT_VOLUME, 1f);
            SFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME, 1f);
            GUIVolume = PlayerPrefs.GetFloat(GUI_VOLUME, 1f);
        }

        private void OnDestroy()
        {
            foreach (EventInstance eventInstance in _eventInstances)
            {
                eventInstance.release();
            }
            _eventInstances.Clear();

            PlayerPrefs.SetFloat(MASTER_VOLUME, MasterVolume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME, MusicVolume);
            PlayerPrefs.SetFloat(ENTITY_VOLUME, EntityVolume);
            PlayerPrefs.SetFloat(ENVIROMENT_VOLUME, EnvironmentVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME, SFXVolume);
            PlayerPrefs.SetFloat(GUI_VOLUME, GUIVolume);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Instance = Instantiate(Resources.Load<AudioManager>("AudioManager"));
            DontDestroyOnLoad(Instance.gameObject);
        }

        private void InitializeBus()
        {
            _masterBus = RuntimeManager.GetBus("bus:/");
            _musicBus = RuntimeManager.GetBus("bus:/Music");
            _entityBus = RuntimeManager.GetBus("bus:/Entity");
            _enviromentBus = RuntimeManager.GetBus("bus:/Enviroment");
            _sfxBus = RuntimeManager.GetBus("bus:/SFX");
            _guiBus = RuntimeManager.GetBus("bus:/GUI");
        }

        private void InitializeMusicEvent()
        {
            CreateInstance(musicEvent, out _musicEventInstance);
            _musicEventInstance.getDescription(out EventDescription eventDescription);
            eventDescription.getParameterDescriptionByName("MusicName", out PARAMETER_DESCRIPTION parameterDescription);
            _musicNameParameterID = parameterDescription.id;
        }

        private void SetBusVolume(ref Bus bus, float volume)
        {
            bus.setVolume(volume);
        }

        public void SetMusic(MusicName musicName)
        {
            _musicEventInstance.setParameterByID(_musicNameParameterID, (float)musicName);
            TryStartMusic();
        }

        public void TryStartMusic()
        {
            if (IsMusicMuted)
                return;

            _musicEventInstance.getPlaybackState(out PLAYBACK_STATE playbackState);
            if (playbackState is PLAYBACK_STATE.STOPPED or PLAYBACK_STATE.STOPPING)
            {
                _musicEventInstance.start();
            }
        }

        public void StopMusic()
        {
            _musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public static void PlayOneShot(EventReference eventReference, Vector3 position = default)
        {
            try
            {
                EventInstance instance = RuntimeManager.CreateInstance(eventReference.Guid);
                instance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
                instance.start();
                instance.release();
            }
            catch (EventNotFoundException)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"Can't find event {eventReference}");
#endif
            }
        }

        public static void GetParameterID(EventInstance eventInstance ,string parameterName, out PARAMETER_ID parameterID)
        {
            try
            {
                eventInstance.getDescription(out EventDescription eventDescription);
                eventDescription.getParameterDescriptionByName(parameterName, out PARAMETER_DESCRIPTION parameterDescription);
                parameterID = parameterDescription.id;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Can't find parameter: {parameterName}\n{e}");
                parameterID = new PARAMETER_ID();
            }
            
        }

        public void CreateInstance(EventReference eventReference, out EventInstance eventInstance)
        {
            eventInstance = RuntimeManager.CreateInstance(eventReference);
            _eventInstances.Add(eventInstance);
        }
    }
}
