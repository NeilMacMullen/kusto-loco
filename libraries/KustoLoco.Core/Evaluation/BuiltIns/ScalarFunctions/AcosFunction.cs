using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Acos")]
internal partial class AcosFunction
{
    private static double Impl(double input) => Math.Acos(input);
}
