using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using Minecraft.Audio;
using NaughtyAttributes;
using System;
using System.Threading;
using UnityEngine;

namespace Minecraft
{
    public class RandomSay : MonoBehaviour
    {
        [SerializeField]
        private EventReference sayEvent;

        [SerializeField, MinMaxSlider(0f, 15f)]
        private Vector2 sayInterval = new Vector2(1f, 5f);

        private CancellationTokenSource cancellationTokenSource;

        private void OnEnable()
        {
            cancellationTokenSource = new CancellationTokenSource();
            SayingRoutine(cancellationTokenSource.Token).Forget();
        }

        private void OnDisable()
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        private async UniTaskVoid SayingRoutine(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(sayInterval.x, sayInterval.y)), cancellationToken: cancellationToken);
                AudioManager.PlayOneShot(sayEvent, transform.position);
            }
        }
    }
}
