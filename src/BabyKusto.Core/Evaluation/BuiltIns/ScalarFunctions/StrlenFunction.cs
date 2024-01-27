namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Functions.Strlen")]
internal partial class StrlenFunction
{
    public long Impl(string s) => s.Length;
}