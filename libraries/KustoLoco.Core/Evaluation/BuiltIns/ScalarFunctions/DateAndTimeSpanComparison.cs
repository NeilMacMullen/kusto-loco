using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class DateAndTimeSpanComparison
{
    internal static DateTime Max(DateTime a, DateTime b)
        => a > b ? a : b;
    internal static TimeSpan Max(TimeSpan a, TimeSpan b)
        => a > b ? a : b;

    internal static DateTime Min(DateTime a, DateTime b)
        => a < b ? a : b;
    internal static TimeSpan Min(TimeSpan a, TimeSpan b)
        => a < b ? a : b;
}
