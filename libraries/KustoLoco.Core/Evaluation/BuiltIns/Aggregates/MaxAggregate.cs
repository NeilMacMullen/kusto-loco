using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Max")]
internal partial class MaxAggregate
{
    internal static int IntImpl(NumericAggregate context, int n)
    {
        context.Total = context.Count == 0 ? n : Math.Max(context.Total, n);
        context.Count++;
        return 0;
    }

    internal static int? IntImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : (int)context.Total;

    internal static long LongImpl(NumericAggregate context, long n)
    {
        context.Total = context.Count == 0 ? n : Math.Max(context.Total, n);
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : (long)context.Total;

    internal static double DoubleImpl(NumericAggregate context, double n)
    {
        context.Total = context.Count == 0 ? n : Math.Max(context.Total, n);
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.Total;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n)
    {
        context.Total = context.Count == 0 ? n.Ticks : Math.Max(context.Total, n.Ticks);
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new TimeSpan((long)context.Total);

    internal static DateTime DtImpl(NumericAggregate context, DateTime n)
    {
        context.Total = context.Count == 0 ? n.Ticks : Math.Max(context.Total, n.Ticks);
        context.Count++;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new DateTime((long)context.Total);
}
