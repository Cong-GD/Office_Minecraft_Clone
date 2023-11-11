using System;
using System.Diagnostics;
using UnityEngine;

public class TimeExcute : IDisposable
{

    public static TimeExcute Start(string message)
    {
#if UNITY_EDITOR
        return new TimeExcute(message);
#else
        return null;
#endif  
    }

    private readonly string _jobName;
    private readonly Stopwatch _stopwatch;

    private TimeExcute(string message)
    {
        _jobName  = message;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        UnityEngine.Debug.Log($"{_jobName} in {_stopwatch.ElapsedMilliseconds} ms".RichText(Color.yellow));
    }
}