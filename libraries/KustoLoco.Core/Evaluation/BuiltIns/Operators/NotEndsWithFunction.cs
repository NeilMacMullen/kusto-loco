using System;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Operators.NotEndsWith")]
internal partial class NotEndsWithFunction
{
    private static bool Impl(string a, string b) => !a.EndsWith(b, StringComparison.InvariantCultureIgnoreCase);
}