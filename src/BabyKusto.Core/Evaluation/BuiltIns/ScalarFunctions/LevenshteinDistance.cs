using BabyKusto.Core.Util;
using Fastenshtein;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class LevenshteinDistance
{
    internal static int Impl(string left, string right)
        => Levenshtein.Distance(left, right);
}