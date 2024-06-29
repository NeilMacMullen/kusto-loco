using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DatetimeDiff")]
internal partial class DatetimeDiffFunction
{
    //TODO - I'm not sure this matches the Kusto implementation
    //It's unclear from the documentation whether things like "day" should be rounded or truncated
    private static long Impl(string period, DateTime a, DateTime b)
    {
        return period switch
        {
            "year" => a.Year - b.Year,
            "month" => (a.Year - b.Year) * 12 + a.Month - b.Month,
            "quarter" => (a.Year - b.Year) * 4 + (a.Month - b.Month) / 3,
            "week" => (long)(a - b).TotalDays / 7,
            "day" => (long)(a - b).TotalDays,
            "hour" => (long)(a - b).TotalHours,
            "minute" => (long)(a - b).TotalMinutes,
            "second" => (long)(a - b).TotalSeconds,
            "millisecond" => (long)(a - b).TotalMilliseconds,
            "microsecond" => (long)(a - b).Ticks / 10,
            "nanosecond" => (a - b).Ticks,
            _ => 0
        };
    }
    
}
