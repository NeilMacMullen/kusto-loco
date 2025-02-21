using System.Text.RegularExpressions;
using KustoLoco.Core.Settings;

namespace Lokql.Engine;

public class BlockInterpolator
{


    private readonly List<KustoSettingsProvider> _settingsStack = [];
    public BlockInterpolator(KustoSettingsProvider coreSettings)
    {
        _settingsStack.Add(coreSettings);
    }

    public void PushSettings(KustoSettingsProvider settings)
    {
        _settingsStack.Add(settings);
    }
    public void PopSettings()
    {
        _settingsStack.RemoveAt(_settingsStack.Count-1);
    }

    public string Interpolate(string query)
    {
        return Regex.Replace(query, @"\$?\$(\(?)([a-zA-Z0-9_\.]+)(\)?)", ReplaceVar);

        string ReplaceVar(Match m)
        {
            var fullMatch =m.Value;
            if(fullMatch.StartsWith("$$"))
                return fullMatch.Substring(1);
            var term = m.Groups[2].Value;
            foreach(var settings in _settingsStack.ToArray().Reverse())
            {
                //cope with unbalance brackets such as bin($abcd) where we want to prevent the
                //losing bracket being lost.
                var variableHasLeadingBracket = m.Groups[1].Value == "(";
                if(settings.HasSetting(term))
                    return
                        variableHasLeadingBracket
                            ? settings.TrySubstitute(term)
                            : settings.TrySubstitute(term) + m.Groups[3].Value;
            }
            return fullMatch;
        }
    }
}
