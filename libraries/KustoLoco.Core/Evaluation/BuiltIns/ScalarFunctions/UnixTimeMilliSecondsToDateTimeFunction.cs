using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.UnixTimeMillisecondsToDateTime")]
internal partial class UnixTimeMilliSecondsToDateTimeFunction
{
    private static DateTime? Impl(double milliSeconds) =>
        DateTime.UnixEpoch + TimeSpan.FromMilliseconds(milliSeconds);
}
