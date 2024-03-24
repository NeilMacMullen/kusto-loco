// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;

// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Bin")]
internal partial class BinFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long IntImpl(int left, int right)
    {
        if (right <= 0)
        {
            // TODO: Should be null (perhaps?)
            return 0;
        }

        var remn = left % right;
        if (remn < 0)
        {
            remn += right;
        }

        return left - remn;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long LongImpl(long left, long right)
    {
        if (right <= 0)
        {
            // TODO: Should be null (perhaps?)
            return 0;
        }

        var remn = left % right;
        if (remn < 0)
        {
            remn += right;
        }

        return left - remn;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double DoubleImpl(double left, double right)
    {
        if (right <= 0)
        {
            // TODO: Should be null (perhaps?)
            return 0;
        }

        return Math.Floor(left / right) * right;
    }

    internal static DateTime TimeImpl(DateTime left, TimeSpan right) =>
        new(BinFunctionLongImpl.LongImpl(left.Ticks, right.Ticks));

    internal static TimeSpan TimeSpanImpl(TimeSpan left, TimeSpan right) =>
        new(BinFunctionLongImpl.LongImpl(left.Ticks, right.Ticks));
}