using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.SumIf")]
internal partial class SumIfAggregate
{
    internal static long IntImpl(NumericAggregate context, int n, bool t)
    {
        if (!t)
            return 0;
        context.LongValue += n;
        context.Count++;
        return 0;
    }

    internal static long? IntImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.LongValue;

    internal static long LongImpl(NumericAggregate context, long n, bool t)
    {
        if (!t)
            return 0;
        context.LongValue += n;
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.LongValue;

    internal static double DoubleImpl(NumericAggregate context, double n, bool t)
    {
        if (!t)
            return 0;
        context.DoubleValue += n;
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.DoubleValue;

    internal static decimal DecimalImpl(NumericAggregate context, decimal n, bool t)
    {
        if (!t)
            return 0;
        context.DecimalValue += n;
        context.Count++;
        return 0;
    }

    internal static decimal? DecimalImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.DecimalValue;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return TimeSpan.Zero;
        context.LongValue += n.Ticks;
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new TimeSpan(context.LongValue);
}
