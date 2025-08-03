//
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Bin")]
internal partial class BinFunction
{
    private static long? IntImpl(int left, int right)
    {
        if (right <= 0)
            return null;

        var remn = left % right;
        if (remn < 0) remn += right;

        return left - remn;
    }

    internal static long? LongImpl(long left, long right)
    {
        if (right <= 0)
            return null;

        var remainder = left % right;
        if (remainder < 0) remainder += right;

        return left - remainder;
    }

    internal static double? DoubleImpl(double left, double right)
    {
        if (right <= 0)
            return null;
        return Math.Floor(left / right) * right;
    }

    internal static DateTime TimeImpl(DateTime left, TimeSpan right)
    {
        var bin = BinFunctionLongImpl.LongImpl(left.Ticks, right.Ticks)!;
        return new DateTime(bin.Value);
    }

    internal static TimeSpan TimeSpanImpl(TimeSpan left, TimeSpan right)
    {
        var bin = BinFunctionLongImpl.LongImpl(left.Ticks, right.Ticks)!;
        return new TimeSpan(bin.Value);
    }
}
