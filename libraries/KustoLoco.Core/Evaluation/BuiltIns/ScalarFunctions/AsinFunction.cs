using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Asin")]
internal partial class AsinFunction
{
    private static double Impl(double input) => Math.Asin(input);
}
