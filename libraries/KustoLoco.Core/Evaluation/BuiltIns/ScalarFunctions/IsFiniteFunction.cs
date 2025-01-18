namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.IsFinite")]
internal partial class IsFiniteFunction
{
    internal static bool Impl(double  v) =>
        double.IsFinite(v);
}
