using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Sum")]
internal partial class SumAggregate
{
    internal static long IntImpl(NumericAggregate context, int n)
    {
        context.LongValue += n;
        context.Count++;
        return 0;
    }

    internal static long? IntImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.LongValue;

    internal static long LongImpl(NumericAggregate context, long n)
    {
        context.LongValue += n;
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.LongValue;

    internal static double DoubleImpl(NumericAggregate context, double n)
    {
        context.DoubleValue += n;
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.DoubleValue;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n)
    {
        context.LongValue += n.Ticks;
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new TimeSpan(context.LongValue);
}
