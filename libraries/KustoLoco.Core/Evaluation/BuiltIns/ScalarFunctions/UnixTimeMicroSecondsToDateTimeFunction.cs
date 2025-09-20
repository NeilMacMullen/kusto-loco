using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.UnixTimeMicrosecondsToDateTime")]
internal partial class UnixTimeMicroSecondsToDateTimeFunction
{
    private static DateTime? Impl(double microSeconds) =>
        DateTime.UnixEpoch + TimeSpan.FromMicroseconds(microSeconds);
}
