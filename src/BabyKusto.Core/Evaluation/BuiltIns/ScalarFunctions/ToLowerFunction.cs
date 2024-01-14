// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ToLower")]
internal partial class ToLowerFunction
{
    private static string Impl(string s) => s.ToLowerInvariant();
}