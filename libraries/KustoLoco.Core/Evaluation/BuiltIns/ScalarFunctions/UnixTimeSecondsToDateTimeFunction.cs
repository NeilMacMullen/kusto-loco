using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.UnixTimeSecondsToDateTime")]
internal partial class UnixTimeSecondsToDateTimeFunction
{
    private static DateTime? Impl(double seconds) =>
        DateTime.UnixEpoch+TimeSpan.FromSeconds(seconds);
}
