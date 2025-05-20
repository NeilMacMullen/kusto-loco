using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.Subtract")]
internal partial class SubtractFunction
{
    private static long IntImpl(int a, int b) => a - b;
    private static long LongImpl(long a, long b) => a - b;
  
    private static double DoubleImpl(double a, double b) => a - b;
    private static decimal DecimalImpl(decimal a, decimal b) => a - b;
    private static TimeSpan TsImpl(TimeSpan a, TimeSpan b) => a - b;
    private static DateTime TsDtImpl(DateTime a, TimeSpan b) => a - b;
    private static TimeSpan DtDtImpl(DateTime a, DateTime b) => a - b;
}
