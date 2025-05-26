using System.Text.RegularExpressions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Extract")]
internal partial class ExtractFunction
{
    private static string Impl(ExtractContext context,string pattern, long captureGroup, string input)
    {
        var regex = context.LastPattern ==pattern
            ? context.LastRegex
            : new Regex(pattern);

        context.LastPattern=pattern;
        context.LastRegex=regex;
        var match = regex.Match(input);
        if (match.Success)
            if (captureGroup < match.Groups.Count)
                return match.Groups[(int)captureGroup].Value;

        return string.Empty;
    }

   
}
internal class ExtractContext
{
    public string LastPattern = string.Empty;
    public Regex LastRegex = new("");
    
}
