using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Atan")]
internal partial class AtanFunction
{
    private static double Impl(double input) => Math.Atan(input);
}
