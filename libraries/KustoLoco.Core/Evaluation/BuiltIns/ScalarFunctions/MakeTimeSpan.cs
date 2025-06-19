using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "make_timespan")]
internal partial class MakeTimeSpan
{
    internal static TimeSpan Impl(long day, long hour, long minute, double second)
        => new((int)day, (int)hour,
            (int)minute, (int)second);

    internal static TimeSpan AImpl(long hour, long minute,double second)
        => MakeTimeSpan.Impl(0, hour, minute, second);
    internal static TimeSpan BImpl(long hour, long minute)
        => MakeTimeSpan.AImpl(hour,minute,0);

}
