using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Operators.Divide")]
internal partial class DivideFunction
{
    private static long? IntImpl(int a, int b)
        => b == 0
               ? null
               : a / b;

    private static long? LongImpl(long a, long b)
        => b == 0
               ? null
               : a / b;

    private static double? DoubleImpl(double a, double b)
        => b == 0
               ? null
               : a / b;

    private static TimeSpan? TsLongImpl(TimeSpan a, long b)
        => b == 0
               ? null
               : a / b;

    private static TimeSpan? TsIntImpl(TimeSpan a, int b)
        => b == 0
               ? null
               : a / b;

    private static double? TsTsImpl(TimeSpan a, TimeSpan b)
        => b == TimeSpan.Zero
               ? null
               : a / b;

    private static TimeSpan? TsDoubleImpl(TimeSpan a, double b)
        => b == 0
               ? null
               : a / b;
}
