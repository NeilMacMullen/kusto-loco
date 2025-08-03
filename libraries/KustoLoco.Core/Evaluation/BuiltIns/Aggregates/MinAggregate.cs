using System;
using System.Collections.Concurrent;
using System.Linq;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Min")]
internal partial class MinAggregate
{
    internal static long IntImpl(NumericAggregate context, int n)
    {
        context.LongValue = context.Count == 0 ? n : Math.Min(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static long? IntImplFinish(ConcurrentBag<NumericAggregate> contextSet)
    {
        var valid = contextSet.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.LongValue).Min() : null;
    }

    internal static long LongImpl(NumericAggregate context, long n)
    {
        context.LongValue = context.Count == 0 ? n : Math.Min(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(ConcurrentBag<NumericAggregate> contextSet)
    {
        var valid = contextSet.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.LongValue).Min() : null;
    }

    internal static double DoubleImpl(NumericAggregate context, double n)
    {
        context.DoubleValue = context.Count == 0 ? n : Math.Min(context.DoubleValue, n);
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(ConcurrentBag<NumericAggregate> contextSet)
    {
        var valid = contextSet.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.DoubleValue).Min() : null;
    }

    internal static decimal DecimalImpl(NumericAggregate context, decimal n)
    {
        context.DecimalValue = context.Count == 0 ? n : Math.Min(context.DecimalValue, n);
        context.Count++;
        return 0;
    }

    internal static decimal? DecimalImplFinish(ConcurrentBag<NumericAggregate> contextSet)
    {
        var valid = contextSet.Where(c => c.Count > 0).ToList();
        return valid.Any() ? valid.Select(c => c.DecimalValue).Min() : null;
    }

    internal static TimeSpan TimeSpanImpl(NumericAggregate context, TimeSpan n)
    {
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Min(context.LongValue, n.Ticks);
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TimeSpanImplFinish(ConcurrentBag<NumericAggregate> contextSet)
    {
        var valid = contextSet.Where(c => c.Count > 0).ToList();
        return valid.Any() ? new TimeSpan(valid.Select(c => c.LongValue).Min()) : null;
    }

    internal static DateTime DateTimeImpl(NumericAggregate context, DateTime n)
    {
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Min(context.LongValue, n.Ticks);
        context.Count++;
        return DateTime.MinValue;
    }

    internal static DateTime? DateTimeImplFinish(ConcurrentBag<NumericAggregate> contextSet)
    {
        var valid = contextSet.Where(c => c.Count > 0).ToList();
        return valid.Any() ? new DateTime(valid.Select(c => c.LongValue).Min(), DateTimeKind.Utc) : null;
    }
}
