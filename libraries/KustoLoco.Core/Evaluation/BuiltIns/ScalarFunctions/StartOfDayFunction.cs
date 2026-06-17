using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfDay")]
internal partial class StartOfDayFunction
{
    internal static DateTime Calculate(DateTime input,long daysOffset) =>
        new DateTime(
            year: input.Year,
            month: input.Month,
            day: input.Day,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind)
            .AddDays(daysOffset);

    private static DateTime Impl(DateTime input) =>
        StartOfDayFunction.Calculate(input, 0);

    private static DateTime IntImpl(DateTime input,int offset) =>
        StartOfDayFunction.Calculate(input, offset);
    private static DateTime LongImpl(DateTime input, long offset) =>
        StartOfDayFunction.Calculate(input, offset);
}
