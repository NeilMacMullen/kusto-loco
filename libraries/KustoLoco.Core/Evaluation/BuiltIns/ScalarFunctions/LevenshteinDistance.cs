// ReSharper disable PartialTypeWithSinglePart

using KustoLoco.Core.LibraryFunctions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "levenshtein")]
internal partial class LevenshteinDistance
{
    internal static int Impl(string left, string right)
        => LevenshteinFunctions.Distance(left, right);
}