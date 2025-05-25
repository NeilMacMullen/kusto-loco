using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.LessThan")]
internal partial class LessThanFunction
{
    private static bool IntImpl(int a, int b) => a < b;
    private static bool LongImpl(long a, long b) => a < b;

    private static bool DoubleImpl(double a, double b) => a < b;
    private static bool DecimalImpl(decimal a, decimal b) => a < b;
    private static bool TsImpl(TimeSpan a, TimeSpan b) => a < b;
    private static bool DtImpl(DateTime a, DateTime b) => a < b;
}
