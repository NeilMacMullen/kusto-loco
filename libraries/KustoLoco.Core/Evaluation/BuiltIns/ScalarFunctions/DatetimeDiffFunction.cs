using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DatetimeDiff")]
internal partial class DatetimeDiffFunction
{
      private static long Impl(string period, DateTime a, DateTime b)
    {
       
        return period.ToLowerInvariant() switch
        {
            DateTimeParts.Year => a.Year - b.Year,
            DateTimeParts.Month => (a.Year - b.Year) * 12 + a.Month - b.Month,
            DateTimeParts.Quarter => (a.Year - b.Year) * 4 + (long)Math.Ceiling((a.Month - b.Month) / 3.0),
            DateTimeParts.Week => (long)(Math.Ceiling((a - b).TotalDays / 7.0)),
            DateTimeParts.Day => (long)Math.Ceiling((a - b).TotalDays),
            DateTimeParts.Hour => (long)Math.Ceiling((a - b).TotalHours),
            DateTimeParts.Minute => (long)Math.Ceiling((a - b).TotalMinutes),
            DateTimeParts.Second => (long)Math.Ceiling((a - b).TotalSeconds),
            DateTimeParts.Millisecond => (long)Math.Ceiling((a - b).TotalMilliseconds),
            DateTimeParts.Microsecond => (long)Math.Ceiling((a - b).TotalMicroseconds),
            DateTimeParts.Nanosecond => (a - b).Ticks * 100,
            _ => 0
        };
    
    }
    
}
