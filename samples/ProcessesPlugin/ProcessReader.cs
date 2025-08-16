using System.Collections.Immutable;
using System.Diagnostics;

namespace ProcessesPlugin;

public static class ProcessReader
{
    public static ImmutableArray<ProcessInfo> GetProcesses() =>
    [
        ..Process
            .GetProcesses()
            .Select(p => new ProcessInfo(p.Id, p.ProcessName, p.Threads.Count, p.WorkingSet64))
    ];
}
