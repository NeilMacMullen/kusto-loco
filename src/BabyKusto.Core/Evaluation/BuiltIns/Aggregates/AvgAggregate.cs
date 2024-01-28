using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Avg")]
internal partial class AvgAggregate
{
    internal static double IntImpl(AvgAcc context, int n)
    {
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? IntImplFinish(AvgAcc context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static double LongImpl(AvgAcc context, long n)
    {
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? LongImplFinish(AvgAcc context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static double DoubleImpl(AvgAcc context, double n)
    {
        context.Count++;
        context.Total += n;
        return 0;
    }

    internal static double? DoubleImplFinish(AvgAcc context) => context.Count == 0
        ? null
        : context.Total / context.Count;

    internal static TimeSpan TsImpl(AvgAcc context, TimeSpan n)
    {
        context.Count++;
        context.Total += n.Ticks;
        return TimeSpan.Zero;
    }


    internal static TimeSpan? TsImplFinish(AvgAcc context) => context.Count == 0
        ? null
        : new TimeSpan((long)context.Total / context.Count);

    internal static DateTime DtImpl(AvgAcc context, DateTime n)
    {
        context.Count++;
        context.Total += n.Ticks;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(AvgAcc context) => context.Count == 0
        ? null
        : new DateTime((long)context.Total / context.Count);
}