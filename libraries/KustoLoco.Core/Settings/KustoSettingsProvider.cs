using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NotNullStrings;

namespace KustoLoco.Core.Settings;

/// <summary>
///     Provides a flexible way of passing settings around the various KustoLoco layers
/// </summary>
/// <remarks>
///     Yes, it's another string/string dictionary with basic type conversion.
///     The benefit of loose typing here is that it's easy to add new settings
///     without changing the interface and it fits well with the idea of allowing the user
///     to change behaviour by typing in values dynamically.  For example, a user in lokqldx could
///     type in "set csv.skipTypeInference yes" to disable automatic type inference.
///     Settings are _mutable_ because we don't want to pass them in for every operation.
///     However. it would be good to find a way to get back to immutability.
/// </remarks>
public class KustoSettingsProvider
{
    private ImmutableHashSet<KustoSettingDefinition> _registeredSettings =
        ImmutableHashSet<KustoSettingDefinition>.Empty;

    private ImmutableDictionary<string, RawKustoSetting> _settings = ImmutableDictionary<string, RawKustoSetting>.Empty;

    public IReadOnlyCollection<KustoSettingDefinition> GetDefinitions()
    {
        return _registeredSettings;
    }

    public void Register(params KustoSettingDefinition[] settings)
    {
        foreach (var setting in settings)
            _registeredSettings = _registeredSettings.Add(setting);
    }

    public void Set(string setting, string value)
    {
        var k = new RawKustoSetting(setting, value);
        _settings = _settings.SetItem(k.Key, k);
    }

    public void Set(string setting, bool value)
    {
        Set(setting, value.ToString());
    }

    public void Set(string setting, int value)
    {
        Set(setting, value.ToString());
    }


    public string Get(KustoSettingDefinition setting)
    {
        var settingName = setting.Name;
        var fb = new RawKustoSetting(settingName, setting.DefaultValue);
        return _settings.GetValueOrDefault(settingName.ToLowerInvariant(), fb).Value;
    }

    /// <summary>
    /// Tries to interpret a string as a setting name 
    /// </summary>
    /// <remarks>
    /// There are a number of places in application code where it's convenient to see if the
    /// supplied string might actually be interpreted as a setting name.  For example
    /// .set abc xyz
    /// .command abc
    /// --> could transform to .command xyz
    /// </remarks>
    public string TrySubstitute(string name)
    {
        var fb = new RawKustoSetting(name, name);
        return _settings.GetValueOrDefault(name.ToLowerInvariant(), fb).Value;
    }

    public string GetOr(string setting,string fallback)
    {
        var fb = new KustoSettingDefinition(setting, string.Empty,fallback,string.Empty);
        return Get(fb);
    }
    /// <summary>
    /// Try to fetch a setting and interpret it as a number (int)
    /// </summary>
    public int GetInt(KustoSettingDefinition setting)
    {
        var s = Get(setting);
        return int.TryParse(s, out var v) ? v : 0;
    }

    /// <summary>
    ///     Try to interpret the setting as a boolean
    /// </summary>
    /// <remarks>
    ///     Try using true/false first, then yes/no or use non-zero numeric value to indicate true
    /// </remarks>
    public bool GetBool(KustoSettingDefinition setting)
    {
        var trueValues = new[] { "true", "yes", "on", "1" };
        var falseValues = new[] { "false", "no", "off", "0" };
        var s = Get(setting).ToLowerInvariant();
        if (trueValues.Contains(s)) return true;
        if (falseValues.Contains(s)) return false;
        return false;
    }

    public IEnumerable<RawKustoSetting> Enumerate()
    {
        return _settings.Values;
    }

    /// <summary>
    ///     Obtain an array of strings from a setting that is a list of paths
    /// </summary>
    /// <remarks>
    ///     Path-lists use the ';' separator and elements are trimmed so it's not possible
    ///     to use paths that start or end with spaces.
    /// </remarks>
    public string[] GetPathList(KustoSettingDefinition name)
    {
        var k = Get(name);
        return k.Tokenize(";");
    }

    /// <summary>
    ///     Reset all settings to their default values
    /// </summary>
    public void Reset()
    {
        _settings = ImmutableDictionary<string, RawKustoSetting>.Empty;
    }
}
