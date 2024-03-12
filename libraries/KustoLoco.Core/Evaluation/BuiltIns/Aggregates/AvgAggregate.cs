using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Avg")]
internal partial class AvgAggregate
{
    internal static double IntImpl(NumericAggregate context, int n)
    {
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? IntImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static double LongImpl(NumericAggregate context, long n)
    {
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? LongImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static double DoubleImpl(NumericAggregate context, double n)
    {
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n)
    {
        context.Count++;
        context.Total += n.Ticks;
        return TimeSpan.Zero;
    }


    internal static TimeSpan? TsImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : new TimeSpan((long)context.Total / context.Count);

    internal static DateTime DtImpl(NumericAggregate context, DateTime n)
    {
        context.Count++;
        context.Total += n.Ticks;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(NumericAggregate context) => context.Count == 0
        ? null
        : new DateTime((long)context.Total / context.Count);
}