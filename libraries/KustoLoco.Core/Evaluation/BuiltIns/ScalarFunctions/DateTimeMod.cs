using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public static class DateTimeMod
{
    public static DateTime Add(string period, long amount, DateTime a)
    {
        var amt = (int)amount;
        return period.ToLowerInvariant() switch
        {
            DateTimeParts.Year =>
                a.AddYears(amt),
            DateTimeParts.Month => a.AddMonths(amt),
            DateTimeParts.Quarter => a.AddMonths(amt * 3),
            DateTimeParts.Week => a.AddDays(amt * 7),
            DateTimeParts.Day => a.AddDays(amt),
            DateTimeParts.Hour => a.AddHours(amt),
            DateTimeParts.Minute => a.AddMinutes(amt),
            DateTimeParts.Second => a.AddSeconds(amt),
            DateTimeParts.Millisecond => a.AddMilliseconds(amt),
            DateTimeParts.Microsecond => a.AddMicroseconds(amt),
            DateTimeParts.Nanosecond => a.AddTicks(amt/100),
            _ => a
        };
    }
}
