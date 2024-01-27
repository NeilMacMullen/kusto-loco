using System;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.Modulo")]
internal partial class ModuloFunction
{
    private static long? IntImpl(int a, int b) => b == 0 ? null : a % b;
    private static long? LongImpl(long a, long b) => b == 0 ? null : a % b;

    private static double? DoubleImpl(double a, double b) => b == 0 ? null : a % b;

    private static TimeSpan? TsImpl(TimeSpan a, TimeSpan b) =>
        b == TimeSpan.Zero ? null : new TimeSpan(b.Ticks % a.Ticks);
}