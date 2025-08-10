using System.Text.RegularExpressions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Extract")]
internal partial class ExtractFunction
{
    private static string Impl(string pattern, long captureGroup, string input)
    {
        var match = Regex.Match(input, pattern, RegexOptions.Compiled);
        var result = string.Empty;
        if (!match.Success) return result;
        if (captureGroup < match.Groups.Count)
            result = match.Groups[(int)captureGroup].Value;
        return result;
    }
}

internal class ExtractContext
{
    public long LastCaptureGroup = 0;
    public string LastInput = string.Empty;
    public string LastPattern = string.Empty;
    public string LastResult = string.Empty;
}
