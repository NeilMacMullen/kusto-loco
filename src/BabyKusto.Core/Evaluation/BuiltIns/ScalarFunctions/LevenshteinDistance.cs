using Fastenshtein;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class LevenshteinDistance
{
    internal static int Impl(string left, string right)
        => Levenshtein.Distance(left, right);
}