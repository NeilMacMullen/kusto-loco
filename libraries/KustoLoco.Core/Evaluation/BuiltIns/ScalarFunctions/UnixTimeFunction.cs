using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "unix_time")]
internal partial class UnixTimeFunction
{
    internal static DateTime SecsImpl(long n)
        => DateTime.UnixEpoch +TimeSpan.FromSeconds(1);
    internal static DateTime TsImpl(TimeSpan offset)
        => DateTime.UnixEpoch + offset;
}

[KustoImplementation(Keyword = "net_time")]
internal partial class NetTimeFunction
{
    internal static DateTime SecsImpl(long n)
        => DateTime.MinValue + TimeSpan.FromSeconds(1);
    internal static DateTime TsImpl(TimeSpan offset)
        => DateTime.MinValue + offset;
}
