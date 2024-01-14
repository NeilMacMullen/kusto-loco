using System;
using AdvancedStringFunctionality;
using SourceGeneratorDependencies;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation]
internal class StringSimilarity
{
    public double Impl(string left, string right)
    {
        if (left.Length == 0 && right.Length == 0)
            return 1.0;

        if (left.Length == 0 || right.Length == 0)
        {
            return 0;
        }

        var distance = LevenshteinFunctions.Distance(left, right);
        var smallestString = Math.Min(left.Length, right.Length);

        var similarity = 1.0 - ((double)distance / smallestString);
        return similarity;
    }
}