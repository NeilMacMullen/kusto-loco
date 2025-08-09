using System;
using System.Globalization;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "todatetimefmt")]
internal partial class ToDateTimeFmtFunction
{
    private static DateTime? Impl(string input,string fmt) => DateTime.TryParseExact(input,fmt,CultureInfo.InvariantCulture, DateTimeStyles.None,out var result) ? result.ToUniversalTime() : null;
}
