using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Sample.ProcessesCli;

public readonly record struct ProcessInfo(int Pid, string Name, int NumThreads, long WorkingSet);

public static class ProcessReader
{
    public static ImmutableArray<ProcessInfo> GetProcesses()
    {
        return Process
            .GetProcesses()
            .Select(p => new ProcessInfo(p.Id, p.ProcessName, p.Threads.Count, p.WorkingSet64))
            .ToImmutableArray();
    }
}