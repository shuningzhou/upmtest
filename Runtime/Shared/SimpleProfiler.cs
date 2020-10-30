using System;
using System.Diagnostics;
using UnityEngine;

public class SProfiler : IDisposable
{
    private Stopwatch _stopwatch = new Stopwatch();
    private Action<TimeSpan> _callback;
    private string _name;

    public SProfiler(string name)
    {
        _name = name;
        _stopwatch.Start();
    }

    public SProfiler(string name, Action<TimeSpan> callback) : this(name)
    {
        _callback = callback;
    }

    public static SProfiler Start(string name, Action<TimeSpan> callback)
    {
        return new SProfiler(name, callback);
    }


    public static SProfiler Start(string name)
    {
        return new SProfiler(name);
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        if (_callback != null)
        {
            _callback(Result);
        }
        else
        {
            UnityEngine.Debug.Log(ResultString);
        }
    }

    public TimeSpan Result
    {
        get { return _stopwatch.Elapsed; }
    }

    public string ResultString
    {
        get
        {
            return $"{_name}: {_stopwatch.Elapsed.TotalMilliseconds}ms";
        }
    }
}