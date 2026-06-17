using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StartOfWeek")]
internal partial class StartOfWeekFunction
{
    internal static DateTime StartOfWeek(DateTime input, long weeksOffset)
    {
        var startOfDay = new DateTime(
            year: input.Year,
            month: input.Month,
            day: input.Day,
            hour: 0,
            minute: 0,
            second: 0,
            kind: input.Kind);
        var startOfWeek = startOfDay.AddDays(-(int)startOfDay.DayOfWeek +weeksOffset*7);
        return startOfWeek;
    }
    private static DateTime Impl(DateTime input)
    {
        return StartOfWeekFunction.StartOfWeek(input, 0);
    }
    private static DateTime IntOffsetImpl(DateTime input,int offset)
    {
        return StartOfWeekFunction.StartOfWeek(input, offset);
    }
    private static DateTime LongOffsetImpl(DateTime input, long offset)
    {
        return StartOfWeekFunction.StartOfWeek(input, offset);
    }

}
