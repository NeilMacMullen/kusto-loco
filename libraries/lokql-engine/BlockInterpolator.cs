using System.Text.RegularExpressions;
using KustoLoco.Core.Settings;

namespace Lokql.Engine;

public class BlockInterpolator
{


    private readonly List<KustoSettingsProvider> _settingsStack = new List<KustoSettingsProvider>();
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
        string rep(Match m)
        {
            var term = m.Groups[1].Value;
            foreach(var settings in _settingsStack.ToArray().Reverse())
            {
              
                var result = settings.TrySubstitute(term);
                if (!string.Equals(result, term, StringComparison.InvariantCultureIgnoreCase))
                    return result;
            }
            return term;
        }

        return Regex.Replace(query, @"\$(\w+)", rep);
    }
}
