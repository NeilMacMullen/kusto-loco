using System;

// ReSharper disable PartialTypeWithSinglePart
namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.NotEndsWithCs")]
internal partial class NotEndsWithCsFunction
{
    private static bool Impl(string a, string b) => !a.EndsWith(b, StringComparison.InvariantCulture);
}