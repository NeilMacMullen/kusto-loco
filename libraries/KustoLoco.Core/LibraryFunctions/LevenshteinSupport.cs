using Fastenshtein;

namespace KustoLoco.Core.LibraryFunctions;

public static class LevenshteinFunctions
{
    public static int Distance(string left, string right)
        => Levenshtein.Distance(left, right);
}