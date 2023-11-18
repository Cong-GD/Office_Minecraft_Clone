using UnityEngine;

public struct Timer
{
    public readonly float endTime;
    public float Current { get; private set; }

    public readonly float CurrentValue => endTime == 0f ? 1f : Mathf.Clamp01(Current / endTime);

    public Timer(float time)
    {
        endTime = Mathf.Max(0f, time);
        Current = 0f;
    }

    public bool Tick(float deltaTime)
    {
        Current += Mathf.Abs(deltaTime);
        return IsDone();
    }

    public void Reset()
    {
        Current = 0f;
    }

    public readonly bool IsDone()
    {
        return Current >= endTime;
    }

}
