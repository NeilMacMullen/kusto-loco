using System;
using System.Collections.Concurrent;
using System.Linq;

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

    internal static long? IntImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.LongValue).Sum() : null;
    }

    internal static long LongImpl(NumericAggregate context, long n)
    {
        context.LongValue += n;
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.LongValue).Sum() : null;
    }

    internal static double DoubleImpl(NumericAggregate context, double n)
    {
        context.DoubleValue += n;
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.DoubleValue).Sum() : null;
    }

    internal static decimal DecimalImpl(NumericAggregate context, decimal n)
    {
        context.DecimalValue += n;
        context.Count++;
        return 0;
    }

    internal static decimal? DecimalImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.DecimalValue).Sum() : null;
    }

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n)
    {
        context.LongValue += n.Ticks;
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? new TimeSpan(valid.Select(c => c.LongValue).Sum()) : null;
    }
}
