using System;
using System.Collections.Concurrent;
using System.Linq;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.MinIf")]
internal partial class MinIfAggregate
{
    internal static long IntImpl(NumericAggregate context, int n, bool t)
    {
        if (!t)
            return 0;
        context.LongValue = context.Count == 0 ? n : Math.Min(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static long? IntImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.LongValue).Min() : null;
    }

    internal static long LongImpl(NumericAggregate context, long n, bool t)
    {
        if (!t)
            return 0;
        context.LongValue = context.Count == 0 ? n : Math.Min(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.LongValue).Min() : null;
    }

    internal static double DoubleImpl(NumericAggregate context, double n, bool t)
    {
        if (!t)
            return 0;
        context.DoubleValue = context.Count == 0 ? n : Math.Min(context.DoubleValue, n);
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.DoubleValue).Min() : null;
    }

    internal static decimal DecimalImpl(NumericAggregate context, decimal n, bool t)
    {
        if (!t)
            return 0;
        context.DecimalValue = context.Count == 0 ? n : Math.Min(context.DecimalValue, n);
        context.Count++;
        return 0;
    }

    internal static decimal? DecimalImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.DecimalValue).Min() : null;
    }

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return TimeSpan.Zero;
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Min(context.LongValue, n.Ticks);
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? new TimeSpan(valid.Select(c => c.LongValue).Min()) : null;
    }

    internal static DateTime DtImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return DateTime.MinValue;
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Min(context.LongValue, n.Ticks);
        context.Count++;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(ConcurrentBag<NumericAggregate> contexts)
    {
        var valid = contexts.Where(c => c.Count > 0).ToList();
        return valid.Any() ? new DateTime(valid.Select(c => c.LongValue).Min(), DateTimeKind.Utc) : null;
    }
}
