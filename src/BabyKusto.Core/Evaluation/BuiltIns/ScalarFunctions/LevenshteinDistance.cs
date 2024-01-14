using AdvancedStringFunctionality;

// ReSharper disable PartialTypeWithSinglePart

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "levenshtein")]
internal partial class LevenshteinDistance
{
    internal static int Impl(string left, string right)
        => LevenshteinFunctions.Distance(left, right);
}