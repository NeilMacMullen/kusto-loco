using System.Text.RegularExpressions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public static class TrimHelpers
{
    public static  string Evaluate(string pattern,string target, bool trimStart, bool trimEnd)
    {
        if (trimStart) target = Regex.Replace(target, "^" + pattern, string.Empty);
        if (trimEnd) target = Regex.Replace(target, pattern + "$", string.Empty);
        return target;
    }
}
