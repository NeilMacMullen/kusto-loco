using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.NotEqual")]
internal partial class NotEqualFunction
{
    private static bool IntImpl(int a, int b) => a != b;
    private static bool LongImpl(long a, long b) => a != b;

    private static bool DoubleImpl(double a, double b) => a != b;
    private static bool DecimalImpl(decimal a, decimal b) => a != b;
    private static bool TsImpl(TimeSpan a, TimeSpan b) => a != b;
    private static bool DtImpl(DateTime a, DateTime b) => a != b;
    private static bool StrImpl(string a, string b) => a != b;
    private static bool BoolImpl(bool a, bool b) => a != b;
    private static bool GuidImpl(Guid a, Guid b) => a != b;
}
