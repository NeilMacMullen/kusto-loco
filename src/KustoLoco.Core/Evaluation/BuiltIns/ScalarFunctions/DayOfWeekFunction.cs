using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DayOfWeek")]
internal partial class DayOfWeekFunction
{
    private static TimeSpan Impl(DateTime input) =>
        // Sunday: 0, Monday: 1, etc...
        // Luckily, this matches how enum DayOfWeek is defined
        TimeSpan.FromDays((int)input.DayOfWeek);
}