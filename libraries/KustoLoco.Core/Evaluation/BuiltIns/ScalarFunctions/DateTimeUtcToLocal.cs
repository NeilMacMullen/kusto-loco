using System;
using NotNullStrings;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DatetimeUtcToLocal")]
internal partial class DateTimeUtcToLocalFunction
{
    private static DateTime? Impl(DateTime input, string tz)
    {
        if (tz.IsBlank())
            return null;
        try
        {
            var tzi =TimeZoneInfo.FindSystemTimeZoneById(tz);
            var d = TimeZoneInfo.ConvertTimeFromUtc(input, tzi);
            return DateTime.SpecifyKind(d, DateTimeKind.Local);
        }
        catch
        {
            return null;
        }
    }
}


