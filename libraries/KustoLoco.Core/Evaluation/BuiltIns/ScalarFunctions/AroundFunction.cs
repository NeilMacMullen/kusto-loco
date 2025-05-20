using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Around")]
internal partial class AroundFunction
{
    public bool Impl(long val,long center,long delta) =>
        Math.Abs(val - center) <= delta;
    public bool DoubleImpl(double  val, double center, double delta) =>
        Math.Abs(val - center) <= delta;
    public bool DecimalImpl(decimal  val, decimal center, decimal delta) =>
        Math.Abs(val - center) <= delta;
    public bool DTImpl(DateTime val, DateTime center, TimeSpan delta) =>
        (val - center).Duration() <= delta;
    public bool TsImpl(TimeSpan val, TimeSpan center, TimeSpan delta) =>
        (val - center).Duration() <= delta;

}
