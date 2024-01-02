using FMODUnity;
using Minecraft.Audio;
using UnityEngine;
using UnityEngine.Events;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    private bool playOnEnable;

    [SerializeField]
    private bool stopOnDisable;

    [SerializeField]
    private MusicName musicName;

    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlayMusic(musicName);
        }
    }

    private void OnDisable()
    {
        if (stopOnDisable)
        {
            AudioManager.Instance.StopMusic();
        }
    }

    public void PlayMusic(MusicName musicName)
    {
        AudioManager.Instance.SetMusic(musicName);
    }
}