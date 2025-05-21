using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DatetimePart")]
internal partial class DatetimePartFunction
{
    private static int Impl(string period, DateTime a)
    {
        return period.ToLowerInvariant() switch
        {
            DateTimeParts.Year => a.Year,
            DateTimeParts.Month => a.Month,
            DateTimeParts.Quarter => 1 + ((a.Month - 1) / 3),
            DateTimeParts.Week => 1 + ((a.DayOfYear - 1) / 7),
            DateTimeParts.Day => a.DayOfYear,
            DateTimeParts.Hour => a.Hour,
            DateTimeParts.Minute => a.Minute,
            DateTimeParts.Second => a.Second,
            DateTimeParts.Millisecond => a.Millisecond,
            DateTimeParts.Microsecond => a.Microsecond,
            DateTimeParts.Nanosecond => (int)a.Ticks*100,
            _ => 0
        };
    }

}
