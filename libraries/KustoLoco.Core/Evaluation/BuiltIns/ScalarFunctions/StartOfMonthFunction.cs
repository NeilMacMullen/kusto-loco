using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfMonth")]
internal partial class StartOfMonthFunction
{
    internal static DateTime Calculate(DateTime input,int monthsOffset) =>
        new DateTime(
            year: input.Year,
            month: input.Month,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind).AddMonths(monthsOffset);

    private static DateTime Impl(DateTime input) =>
        StartOfMonthFunction.Calculate(input,0);
    private static DateTime IntImpl(DateTime input,int monthsOffset) =>
        StartOfMonthFunction.Calculate(input, monthsOffset);

    private static DateTime LongImpl(DateTime input, long monthsOffset) =>
        StartOfMonthFunction.Calculate(input, (int) monthsOffset);

}
