using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.Multiply")]
internal partial class MultiplyFunction
{
    private static long IntImpl(int a, int b) => a * b;

    private static long LongImpl(long a, long b) => a * b;
    private static double DoubleImpl(double a, double b) => a * b;
    private static TimeSpan TsLongImpl(TimeSpan a, long b) => a * b;
    private static TimeSpan LongTsImpl(long a, TimeSpan b) => a * b;

    private static TimeSpan TsDoubleImpl(TimeSpan a, double b) => a * b;
    private static TimeSpan DoubleTsImpl(double a, TimeSpan b) => a * b;
}