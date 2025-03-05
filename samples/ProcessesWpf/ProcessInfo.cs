namespace ProcessesWpf;

public readonly record struct ProcessInfo(int Pid, string Name, int NumThreads, long WorkingSet);
