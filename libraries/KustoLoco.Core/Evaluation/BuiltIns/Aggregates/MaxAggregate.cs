using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.Max")]
internal partial class MaxAggregate
{
    internal static int IntImpl(NumericAggregate context, int n)
    {
        context.LongValue = context.Count == 0 ? n : Math.Max(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static int? IntImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : (int)context.LongValue;

    internal static long LongImpl(NumericAggregate context, long n)
    {
        context.LongValue = context.Count == 0 ? n : Math.Max(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.LongValue;

    internal static double DoubleImpl(NumericAggregate context, double n)
    {
        context.DoubleValue = context.Count == 0 ? n : Math.Max(context.DoubleValue, n);
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.DoubleValue;

    internal static decimal DecimalImpl(NumericAggregate context, decimal n)
    {
        context.DecimalValue = context.Count == 0 ? n : Math.Max(context.DecimalValue, n);
        context.Count++;
        return 0;
    }

    internal static decimal? DecimalImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.DecimalValue;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n)
    {
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Max(context.LongValue, n.Ticks);
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new TimeSpan(context.LongValue);

    internal static DateTime DtImpl(NumericAggregate context, DateTime n)
    {
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Max(context.LongValue, n.Ticks);
        context.Count++;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new DateTime(context.LongValue,DateTimeKind.Utc);
}
