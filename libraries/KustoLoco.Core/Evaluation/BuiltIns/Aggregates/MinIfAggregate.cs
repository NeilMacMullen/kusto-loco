using System;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Aggregates.MinIf")]
internal partial class MinIfAggregate
{
    internal static int IntImpl(NumericAggregate context, int n, bool t)
    {
        if (!t)
            return 0;
        context.LongValue = context.Count == 0 ? n : Math.Min(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static int? IntImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : (int)context.LongValue;

    internal static long LongImpl(NumericAggregate context, long n, bool t)
    {
        if (!t)
            return 0;
        context.LongValue = context.Count == 0 ? n : Math.Min(context.LongValue, n);
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : (long)context.LongValue;

    internal static double DoubleImpl(NumericAggregate context, double n, bool t)
    {
        if (!t)
            return 0;
        context.DoubleValue = context.Count == 0 ? n : Math.Min(context.DoubleValue, n);
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.DoubleValue;


    internal static decimal DecimalImpl(NumericAggregate context, decimal n, bool t)
    {
        if (!t)
            return 0;
        context.DecimalValue = context.Count == 0 ? n : Math.Min(context.DecimalValue, n);
        context.Count++;
        return 0;
    }

    internal static decimal? DecimalImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.DecimalValue;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return TimeSpan.Zero;
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Min(context.LongValue, n.Ticks);
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new TimeSpan((long)context.LongValue);

    internal static DateTime DtImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return DateTime.MinValue;
        context.LongValue = context.Count == 0 ? n.Ticks : Math.Min(context.LongValue, n.Ticks);
        context.Count++;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new DateTime((long)context.LongValue, DateTimeKind.Utc);
}
