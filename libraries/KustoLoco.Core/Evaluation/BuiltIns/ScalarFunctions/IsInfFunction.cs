namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.IsInf")]
internal partial class IsInfFunction
{
    internal static bool Impl(double v) =>
        double.IsInfinity(v);
}

[KustoImplementation(Keyword = "Functions.IsNan")]
internal partial class IsNanFunction
{
    internal static bool Impl(double v) =>
        double.IsNaN(v);
}


