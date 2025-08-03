using System;
using System.Collections.Concurrent;
using System.Linq;

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
        context.DoubleValue += n;
        return 0;
    }

    internal static double? IntImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        var totalCount = valid.Sum(c => c.Count);
        return valid.Any() ? valid.Sum(c => c.DoubleValue) / totalCount : null;
    }

    internal static double LongImpl(NumericAggregate context, long n, bool t)
    {
        if (!t)
            return 0;
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

    internal static double DoubleImpl(NumericAggregate context, double n, bool t)
    {
        if (!t)
            return 0;
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

    internal static decimal DecimalImpl(NumericAggregate context, decimal n, bool t)
    {
        if (!t)
            return 0;
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

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return TimeSpan.Zero;
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

    internal static DateTime DtImpl(NumericAggregate context, DateTime n, bool t)
    {
        if (!t)
            return DateTime.MinValue;
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
