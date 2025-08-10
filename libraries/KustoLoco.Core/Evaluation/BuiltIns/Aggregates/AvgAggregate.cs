using System;
using System.Collections.Concurrent;
using System.Linq;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Avg")]
internal partial class AvgAggregate
{
    internal static double IntImpl(NumericAggregate context, int n)
    {
        context.Count++;
        context.DoubleValue += n;
        return 0;
    }

    internal static double? IntImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        var totalCount = valid.Sum(c => c.Count);
        return valid.Any() ? valid.Sum(c => c.DoubleValue) / totalCount : null;
    }

    internal static double LongImpl(NumericAggregate context, long n)
    {
        context.Count++;
        context.DoubleValue += n;
        return 0;
    }

    internal static double? LongImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        var totalCount = valid.Sum(c => c.Count);
        return valid.Any() ? valid.Sum(c => c.DoubleValue) / totalCount : null;
    }

    internal static double DoubleImpl(NumericAggregate context, double n)
    {
        context.Count++;
        context.DoubleValue += n;
        return 0;
    }

    internal static double? DoubleImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        var totalCount = valid.Sum(c => c.Count);
        return valid.Any() ? valid.Sum(c => c.DoubleValue) / totalCount : null;
    }

    internal static decimal DecimalImpl(NumericAggregate context, decimal n)
    {
        context.Count++;
        context.DecimalValue += n;
        return 0;
    }

    internal static decimal? DecimalImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        var totalCount = valid.Sum(c => c.Count);
        return valid.Any() ? valid.Sum(c => c.DecimalValue) / totalCount : null;
    }

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n)
    {
        context.Count++;
        context.DoubleValue += n.Ticks;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        var totalCount = valid.Sum(c => c.Count);
        return valid.Any()
            ? new TimeSpan((long)(valid.Sum(c => c.DoubleValue) / totalCount))
            : null;
    }

    internal static DateTime DtImpl(NumericAggregate context, DateTime n)
    {
        context.Count++;
        context.DoubleValue += n.Ticks;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        var totalCount = valid.Sum(c => c.Count);
        return valid.Any()
            ? new DateTime((long)(valid.Sum(c => c.DoubleValue) / totalCount), DateTimeKind.Utc)
            : null;
    }
}
