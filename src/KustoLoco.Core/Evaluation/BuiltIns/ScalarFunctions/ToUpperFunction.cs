// ReSharper disable PartialTypeWithSinglePart

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToUpper")]
internal partial class ToUpperFunction
{
    private static string Impl(string s) => s.ToUpperInvariant();
}