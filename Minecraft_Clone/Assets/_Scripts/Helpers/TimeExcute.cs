using System;
using System.Diagnostics;

public class TimeExcute : IDisposable
{

    public static TimeExcute Start(string message, params object[] args)
    {
#if UNITY_EDITOR
        return new TimeExcute(message, args);
#else
        return null;
#endif  
    }

    private readonly string _message;
    private readonly object[] _args;
    private readonly Stopwatch _stopwatch;

    private TimeExcute(string message, params object[] args)
    {
        _message  = message;
        _args = args;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        UnityEngine.Debug.Log($"{string.Format(_message, _args)} in {_stopwatch.ElapsedMilliseconds} ms");
    }
}