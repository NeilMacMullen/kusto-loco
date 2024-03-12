using System.Text.RegularExpressions;

// ReSharper disable PartialTypeWithSinglePart
namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <remarks>
///     This uses the built-in .NET Regex implementation, which isn't strictly compatible with Kusto's RE2.
///     See: https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/re2
/// </remarks>
[KustoImplementation(Keyword = "Operators.MatchRegex")]
internal partial class MatchRegexFunction
{
    internal static bool Impl(MatchRegexContext context, string str, string pattern)
    {
        if (context.LastInput == str && context.LastPattern == pattern)
            return context.LastResult;
        //we may be able to reuse the regex at least
        if (context.LastPattern != pattern)
        {
            context.LastRegex = new Regex(pattern);
            context.LastPattern = pattern;
        }

        return context.LastRegex.IsMatch(str);
    }
}

public class MatchRegexContext
{
    public string LastInput = string.Empty;
    public string LastPattern = string.Empty;
    public Regex LastRegex = new(string.Empty);
    public bool LastResult;
}