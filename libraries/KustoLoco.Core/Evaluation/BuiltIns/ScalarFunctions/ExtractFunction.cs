using System.Text.RegularExpressions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.Extract")]
internal partial class ExtractFunction
{
    private static string Impl(ExtractContext context, string pattern, long captureGroup, string input)
    {
        if (context.LastPattern == pattern &&
            context.LastInput == input &&
            context.LastCaptureGroup == captureGroup)
            return context.LastResult;
        context.LastPattern = pattern;
        context.LastInput = input;
        context.LastCaptureGroup=captureGroup;

        var match = Regex.Match(input, pattern, RegexOptions.Compiled);
        var result = string.Empty;
        if (match.Success)
            if (captureGroup < match.Groups.Count)
                result = match.Groups[(int)captureGroup].Value;
        context.LastResult = result;
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
