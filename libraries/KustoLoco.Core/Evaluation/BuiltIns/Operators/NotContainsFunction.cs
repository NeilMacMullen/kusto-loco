using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.NotContains")]
internal partial class NotContainsFunction
{
    private static bool Impl(string a, string b) => !a.Contains(b, StringComparison.InvariantCultureIgnoreCase);
}