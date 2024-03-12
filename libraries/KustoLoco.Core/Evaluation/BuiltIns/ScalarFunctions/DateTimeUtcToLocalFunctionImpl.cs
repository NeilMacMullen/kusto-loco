using System;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DateTimeUtcToLocalFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var date = (DateTime?)arguments[0].Value;
        var tz = (string?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.DateTime, Impl(date, tz));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var dates = (TypedBaseColumn<DateTime?>)arguments[0].Column;
        var tzs = (TypedBaseColumn<string?>)arguments[1].Column;
        var data = new DateTime?[dates.RowCount];
        for (var i = 0; i < dates.RowCount; i++)
        {
            data[i] = Impl(dates[i], tzs[i]);
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    private static DateTime? Impl(DateTime? input, string? tz)
    {
        if (tz is null) return null;
        if (input is null) return null;
        //TODO - it's ridiculously expensive to do this for every call
        // - introduce caching
        try
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById(tz);
            var d = TimeZoneInfo.ConvertTimeFromUtc(input.Value, tzi);
            return DateTime.SpecifyKind(d, DateTimeKind.Local);
        }
        catch
        {
            return null;
        }
    }
}