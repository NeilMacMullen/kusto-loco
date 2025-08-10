namespace KustoLoco.Core.Diagnostics;

public readonly record struct LogEvent(long Ticks, string Message,long DeltaMs);
