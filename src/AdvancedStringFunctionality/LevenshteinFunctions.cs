using Fastenshtein;

#pragma warning disable CS8604 // Possible null reference argument.

namespace AdvancedStringFunctionality;

public static class LevenshteinFunctions
{
    public static int Distance(string left, string right)
        => Levenshtein.Distance(left, right);
}