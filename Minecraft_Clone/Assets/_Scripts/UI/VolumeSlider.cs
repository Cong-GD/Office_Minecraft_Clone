using Minecraft.Audio;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Minecraft
{
    public class VolumeSlider : MonoBehaviour
    {
        public enum VolumeType
        {
            Master,
            Music,
            SFX,
            GUI,
            Environment,
            Entity,
        }

        [SerializeField]
        private VolumeType volumeType;

        [SerializeField]
        private Slider slider;

        [SerializeField]
        private TextMeshProUGUI text;

        private void OnEnable()
        {
            if(slider != null)
            {
                slider.value = GetVolume(volumeType);
                SetText(slider.value);
                slider.onValueChanged.AddListener(SetVolume);
            }   
        }

        private void OnDisable()
        {
            if (slider != null)
            {
                slider.onValueChanged.RemoveListener(SetVolume);
            }
        }

        private void SetVolume(float value)
        {
            SetVolume(volumeType, value);
            SetText(value);
        }

        private void SetText(float value)
        {
            int percent = Mathf.RoundToInt(value * 100);
            if(percent == 0)
            {
                text.text = $"{volumeType}: OFF";
            }
            else
            {
                text.text = $"{volumeType}: {percent}%";
            }
        }

        private void SetVolume(VolumeType volumeType, float value)
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    AudioManager.Instance.MasterVolume = value;
                    break;
                case VolumeType.Music:
                    AudioManager.Instance.MusicVolume = value;
                    break;
                case VolumeType.SFX:
                    AudioManager.Instance.SFXVolume = value;
                    break;
                case VolumeType.GUI:
                    AudioManager.Instance.GUIVolume = value;
                    break;
                case VolumeType.Environment:
                    AudioManager.Instance.EnvironmentVolume = value;
                    break;
                case VolumeType.Entity:
                    AudioManager.Instance.EntityVolume = value;
                    break;
            }
        }

        private float GetVolume(VolumeType volumeType)
        {
            return volumeType switch
            {
                VolumeType.Master => AudioManager.Instance.MasterVolume,
                VolumeType.Music => AudioManager.Instance.MusicVolume,
                VolumeType.SFX => AudioManager.Instance.SFXVolume,
                VolumeType.GUI => AudioManager.Instance.GUIVolume,
                VolumeType.Environment => AudioManager.Instance.EnvironmentVolume,
                VolumeType.Entity => AudioManager.Instance.EntityVolume,
                _ => 0,
            };
        }
    }

}
