using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.UnixTimeNanosecondsToDateTime")]
internal partial class UnixTimeNanoSecondsToDateTimeFunction
{
    private static DateTime? Impl(double nanoSeconds) =>
        DateTime.UnixEpoch + TimeSpan.FromTicks((long)(nanoSeconds/TimeSpan.NanosecondsPerTick));
}
