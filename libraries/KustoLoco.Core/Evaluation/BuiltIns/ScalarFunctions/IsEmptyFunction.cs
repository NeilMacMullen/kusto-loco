namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Functions.IsEmpty")]
internal partial class IsEmptyFunction
{
    internal static bool Impl(string s) => s.Length == 0;
}


// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Functions.IsFinite")]
internal partial class IsFiniteFunction
{
    internal static bool Impl(double  v) =>
        double.IsFinite(v);
}


