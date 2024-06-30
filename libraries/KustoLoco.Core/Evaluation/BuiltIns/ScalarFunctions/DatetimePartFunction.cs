using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DatetimePart")]
internal partial class DatetimePartFunction
{
   
    private static int Impl(string period, DateTime a)
    {
        return period.ToLowerInvariant() switch
        {
            "year" => a.Year,
            "month" => a.Month,
            "quarter" => 1+((a.Month-1)/3),
            "week" => 1+((a.DayOfYear-1)/7),
            "day" => a.DayOfYear,
            "hour" => a.Hour,
            "minute" => a.Minute,
            "second" => a.Second,
            "millisecond" => a.Millisecond,
            "microsecond" => a.Microsecond,
            "nanosecond" => (int) a.Ticks,
            _ => 0
        };
    }

}
