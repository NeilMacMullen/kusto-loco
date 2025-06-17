using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.UnaryMinus")]
internal partial class UnaryMinusFunction
{
    private static int IntImpl(int a) => -a;
    private static long LongImpl(long a) => -a;
    private static double DoubleImpl(double a) => -a;
    private static decimal DecimalImpl(decimal a) => -a;
    private static TimeSpan TsImpl(TimeSpan a) => -a;
}
