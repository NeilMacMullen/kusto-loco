using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.Add")]
internal partial class AddFunction
{
    private static long IntImpl(int a, int b) => a + b;
    private static long LongImpl(long a, long b) => a + b;

    private static double DoubleImpl(double a, double b) => a + b;
    private static TimeSpan TsImpl(TimeSpan a, TimeSpan b) => a + b;
    private static DateTime TsDtImpl(DateTime a, TimeSpan b) => a + b;
    private static DateTime DtTsImpl(TimeSpan a, DateTime b) => b + a;
}