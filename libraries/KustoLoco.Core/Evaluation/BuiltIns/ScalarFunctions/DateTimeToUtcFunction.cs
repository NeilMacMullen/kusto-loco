using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "datetime_to_utc")]
internal partial class DateTimeToUtcFunction
{
    private static DateTime Impl(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Unspecified)
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        else
        if (dt.Kind == DateTimeKind.Local)
            dt = dt.ToUniversalTime();

        return dt;
    }
}
