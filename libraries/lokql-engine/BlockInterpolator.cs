using System.Text.RegularExpressions;
using KustoLoco.Core.Settings;

namespace Lokql.Engine;

/// <summary>
///     Provides interpolation of setting values
/// </summary>
/// <remarks>
///     Strings such as "abc $s $(val)prefix can be interpolated using
///     this class
/// </remarks>
public static class BlockInterpolator
{
    public static string Interpolate(string query, KustoSettingsProvider settings)
    {
        return Regex.Replace(query, @"\$?\$(\(?)([a-zA-Z0-9_\.]+)(\)?)", ReplaceVar);

        string ReplaceVar(Match m)
        {
            var fullMatch = m.Value;
            if (fullMatch.StartsWith("$$"))
                return fullMatch.Substring(1);
            var term = m.Groups[2].Value;

            //cope with unbalance brackets such as bin($abcd) where we want to prevent the
            //closing bracket being lost.
            var variableHasLeadingBracket = m.Groups[1].Value == "(";
            if (settings.HasSetting(term))
                return
                    variableHasLeadingBracket
                        ? settings.TrySubstitute(term)
                        : settings.TrySubstitute(term) + m.Groups[3].Value;

            return fullMatch;
        }
    }
}
