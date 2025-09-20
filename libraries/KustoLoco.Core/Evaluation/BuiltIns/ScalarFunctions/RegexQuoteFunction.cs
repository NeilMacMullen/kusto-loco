using System.Text.RegularExpressions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.RegexQuote")]
internal partial class RegexQuoteFunction
{
    public string Impl(string s) => Regex.Escape(s);
}
