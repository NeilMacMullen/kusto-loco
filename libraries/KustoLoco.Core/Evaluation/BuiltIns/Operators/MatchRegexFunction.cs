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
    internal static bool Impl(string str, string pattern) => Regex.IsMatch(str, pattern);
}
