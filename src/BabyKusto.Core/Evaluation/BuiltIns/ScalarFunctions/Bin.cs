// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class BinIntFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long Impl(int left, int right)
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
}

[KustoImplementation]
internal class BinLongFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long Impl(long left, long right)
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
}

[KustoImplementation]
internal class BinDoubleFunction
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Impl(double left, double right)
    {
        if (right <= 0)
        {
            // TODO: Should be null (perhaps?)
            return 0;
        }

        return Math.Floor(left / right) * right;
    }
}

[KustoImplementation]
internal class BinDateTimeTimeSpanFunction
{
    internal static DateTime Impl(DateTime left, TimeSpan right) =>
        new(BinLongFunctionImpl.Impl(left.Ticks, right.Ticks));
}