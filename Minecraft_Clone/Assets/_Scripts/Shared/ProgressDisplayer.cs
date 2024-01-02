using System;
using UnityEngine;
using UnityEngine.Events;

public class ProgressDisplayer : MonoBehaviour, IProgress<float>
{
    [field: SerializeField]
    public UnityEvent OnEnable { get; private set; } = new UnityEvent();

    [field: SerializeField]
    public UnityEvent OnDisable { get; private set; } = new UnityEvent();

    [field: SerializeField]
    public UnityEvent<float> OnValueChange { get; private set; } = new UnityEvent<float>();

    public void Enable() => OnEnable.Invoke();
    public void Disable() => OnDisable.Invoke();
    public void SetValue(float value) => OnValueChange.Invoke(value);

    public void Report(float value)
    {
        SetValue(value);
    }
}
