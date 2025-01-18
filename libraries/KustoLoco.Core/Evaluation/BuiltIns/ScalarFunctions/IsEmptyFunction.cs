namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Functions.IsEmpty")]
internal partial class IsEmptyFunction
{
    internal static bool Impl(string s) => s.Length == 0;
}


// ReSharper disable PartialTypeWithSinglePart
