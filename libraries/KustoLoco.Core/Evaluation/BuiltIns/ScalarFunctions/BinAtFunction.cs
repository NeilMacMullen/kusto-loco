using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.BinAt")]
internal partial class BinAtFunction
{
    private static long? IntImpl(int left, int right,int at)
    {
        if (right <= 0)
            return null;

        var atAdjust = at % right;
        var remn = (left-atAdjust) % right;
        if (remn < 0) remn += right;

        return left - remn;
    }

    internal static long? LongImpl(long left, long right,long at)
    {
        if (right <= 0)
            return null;
        var atAdjust = at % right;
        var remainder = (left-atAdjust) % right;
        if (remainder < 0) remainder += right;

        return left - remainder;
    }

    internal static double? DoubleImpl(double left, double right,double at)
    {
        if (right <= 0)
            return null;
        var atAdjust = at % right;
        return (Math.Floor((left-atAdjust) / right) * right) +atAdjust;
    }

    internal static DateTime TimeImpl(DateTime left, TimeSpan right,DateTime at)
    {
        var atAdjust = at.Ticks % right.Ticks;
        var bin = BinFunctionLongImpl.LongImpl(left.Ticks-atAdjust, right.Ticks)!;
        var x = bin.Value +atAdjust;
        return new DateTime(x);
    }

    internal static TimeSpan TimeSpanImpl(TimeSpan left, TimeSpan right,TimeSpan at)
    {
        var atAdjust = at.Ticks % right.Ticks;
        var bin = BinFunctionLongImpl.LongImpl(left.Ticks-atAdjust, right.Ticks)!;
        var x = bin.Value + atAdjust;
        return new TimeSpan(x);
    }
}
