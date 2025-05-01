using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.DatetimeAdd")]
internal partial class DatetimeAddFunction
{
    private static DateTime Impl(string period,long amount, DateTime a)
    {
        return DateTimeMod.Add(period, amount, a);
    }

}
