using System.Text.RegularExpressions;
using KustoLoco.Core.Settings;

namespace Lokql.Engine;

public class BlockInterpolator
{
    private readonly KustoSettingsProvider _coreSettings;

    public BlockInterpolator(KustoSettingsProvider coreSettings)
    {
        _coreSettings = coreSettings;
    }

    public void AddLayer(KustoSettingsProvider newSettings) => _coreSettings.AddLayer(newSettings);

    public void PopSettings() => _coreSettings.Pop();

    public string Interpolate(string query)
    {
        return Regex.Replace(query, @"\$?\$(\(?)([a-zA-Z0-9_\.]+)(\)?)", ReplaceVar);

        string ReplaceVar(Match m)
        {
            var fullMatch = m.Value;
            if (fullMatch.StartsWith("$$"))
                return fullMatch.Substring(1);
            var term = m.Groups[2].Value;
            var settings = _coreSettings;

            //cope with unbalance brackets such as bin($abcd) where we want to prevent the
            //losing bracket being lost.
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
