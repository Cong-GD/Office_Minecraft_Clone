using System;
using UnityEngine;

public class Benmark
{
    DateTime startTime;
    DateTime endTime;

    public void Start()
    {
        startTime = DateTime.Now;
    }

    public void Stop()
    {
        endTime = DateTime.Now;
    }

    public void PrintResult(string methodName)
    {
        var res = endTime - startTime;
        Debug.Log($"{methodName} run in {res.TotalMilliseconds:n0} miliseconds");
    }

}