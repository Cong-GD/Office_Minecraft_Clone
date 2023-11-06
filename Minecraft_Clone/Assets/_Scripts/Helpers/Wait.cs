using System;
using System.Collections.Generic;
using UnityEngine;

public static class Wait
{
    private const int MAX_INTANCE_COUNT = 500;
    private const float LIMIT_TIME = 0.001f;

    private static readonly WaitForEndOfFrame _waitForEndOfFrame = new();
    private static readonly WaitForFixedUpdate _waitForFixedUpdate = new();
    private static readonly Dictionary<float, WaitForSeconds> _waitForSecondsMap = new();
    private static readonly Dictionary<float, WaitForSecondsRealtime> _waitForSecondsInRealTimeMap = new();

    public static WaitForEndOfFrame ForEndOfFrame() => _waitForEndOfFrame;

    public static WaitForFixedUpdate ForFixedUpdate() => _waitForFixedUpdate;

    public static YieldInstruction ForNextFrame() => null;

    public static WaitForSeconds ForSeconds(float seconds)
    {
        if(!_waitForSecondsMap.TryGetValue(seconds, out var wait))
        {
            if(seconds < LIMIT_TIME)
            {
                return null;
            }
            wait = new WaitForSeconds(seconds);
            _waitForSecondsMap[seconds] = wait;
            if(_waitForSecondsMap.Count > MAX_INTANCE_COUNT)
            {
                _waitForSecondsMap.Clear();
            }
        }
        return wait;
    }
    public static WaitForSecondsRealtime ForSecondsInRealTime(float seconds)
    {
        if (!_waitForSecondsInRealTimeMap.TryGetValue(seconds, out var wait))
        {
            if (seconds < LIMIT_TIME)
            {
                return null;
            }
            wait = new WaitForSecondsRealtime(seconds);
            _waitForSecondsInRealTimeMap[seconds] = wait;
            if (_waitForSecondsInRealTimeMap.Count > MAX_INTANCE_COUNT)
            {
                _waitForSecondsInRealTimeMap.Clear();
            }
        }
        return wait;
    }

    public static WaitUntil Until(Func<bool> predicate)
    {
        return new WaitUntil(predicate);
    }

    public static WaitWhile While(Func<bool> predicate)
    {
        return new WaitWhile(predicate);
    }
}
