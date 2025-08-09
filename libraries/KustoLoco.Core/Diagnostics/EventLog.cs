using System;
using System.Collections.Generic;

namespace KustoLoco.Core.Diagnostics;

public static class EventLog
{
    private static long LastMs = 0;
    private static readonly List<LogEvent> _events = [];
    public static void Log(string message)
    {
        var now = DateTime.UtcNow.Ticks;
        ; var nowMs=now/(TimeSpan.TicksPerMillisecond);
        _events.Add(new LogEvent(now, message,nowMs-LastMs));
        LastMs = nowMs;
    }

    public static LogEvent[] GetEvents() => _events.ToArray();
    public static void Clear() => _events.Clear();
}
