namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ReSharper disable PartialTypeWithSinglePart
[KustoImplementation(Keyword = "Functions.IsNotEmpty")]
internal partial class IsNotEmptyFunction
{
    internal static bool Impl(string s) => s is { Length: > 0 };
}
