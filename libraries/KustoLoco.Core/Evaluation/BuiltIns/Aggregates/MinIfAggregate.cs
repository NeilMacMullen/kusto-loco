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
        context.Total = context.Count == 0 ? n : Math.Min(context.Total, n);
        context.Count++;
        return 0;
    }

    internal static int? IntImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : (int)context.Total;

    internal static long LongImpl(NumericAggregate context, long n, bool t)
    {
        if (!t)
            return 0;
        context.Total = context.Count == 0 ? n : Math.Min(context.Total, n);
        context.Count++;
        return 0;
    }

    internal static long? LongImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : (long)context.Total;

    internal static double DoubleImpl(NumericAggregate context, double n, bool t)
    {
        if (!t)
            return 0;
        context.Total = context.Count == 0 ? n : Math.Min(context.Total, n);
        context.Count++;
        return 0;
    }

    internal static double? DoubleImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : context.Total;

    internal static TimeSpan TsImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return TimeSpan.Zero;
        context.Total = context.Count == 0 ? n.Ticks : Math.Min(context.Total, n.Ticks);
        context.Count++;
        return TimeSpan.Zero;
    }

    internal static TimeSpan? TsImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new TimeSpan((long)context.Total);

    internal static DateTime DtImpl(NumericAggregate context, TimeSpan n, bool t)
    {
        if (!t)
            return DateTime.MinValue;
        context.Total = context.Count == 0 ? n.Ticks : Math.Min(context.Total, n.Ticks);
        context.Count++;
        return DateTime.MinValue;
    }

    internal static DateTime? DtImplFinish(NumericAggregate context)
        => context.Count == 0 ? null : new DateTime((long)context.Total, DateTimeKind.Utc);
}
