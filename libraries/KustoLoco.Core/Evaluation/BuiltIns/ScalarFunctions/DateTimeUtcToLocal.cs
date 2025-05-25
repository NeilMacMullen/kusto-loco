using System;
using NotNullStrings;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DatetimeUtcToLocal")]
internal partial class DateTimeUtcToLocalFunction
{
    private static DateTime? Impl(DTContext context,DateTime input, string tz)
    {
        if (tz.IsBlank())
            return null;
        try
        {
            //try to avoid repeated lookups.  Common case
            //will be a single timezone
            var tzi =
            (context.LastString == tz)
                ? context.LastInfo
                : TimeZoneInfo.FindSystemTimeZoneById(tz);
            
            context.LastInfo = tzi;
            context.LastString = tz;

            var d = TimeZoneInfo.ConvertTimeFromUtc(input, tzi);
            return DateTime.SpecifyKind(d, DateTimeKind.Local);
        }
        catch
        {
            return null;
        }
    }
}

internal class DTContext
{
    public string LastString = string.Empty;
    public TimeZoneInfo LastInfo=TimeZoneInfo.Local;
}

