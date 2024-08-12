using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.AvgIf")]
internal partial class AvgIfAggregate
{
    internal static double IntImpl(NumericAggregate context, int n, bool t)
    {
        if (!t)
            return 0;
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? IntImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static double LongImpl(NumericAggregate context, long n, bool t)
    {
        if (!t)
            return 0;
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? LongImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static double DoubleImpl(NumericAggregate context, double n, bool t)
    {
        if (!t)
            return 0;
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return TimeSpan.Zero;
        context.Count++;
        context.Total += n.Ticks;
        return TimeSpan.Zero;
    }


    internal static TimeSpan? TsImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : new TimeSpan((long)context.Total / context.Count);

    internal static DateTime DtImpl(NumericAggregate context, DateTime n, bool t)
    {
        if (!t)
            return DateTime.MinValue;
        context.Count++;
        context.Total += n.Ticks;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : new DateTime((long)context.Total / context.Count,DateTimeKind.Utc);
}
