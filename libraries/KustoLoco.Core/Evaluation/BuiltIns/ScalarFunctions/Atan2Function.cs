using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Atan2")]
internal partial class Atan2Function
{
    private static double Impl(double y,double x) => Math.Atan2(y,x);
}
