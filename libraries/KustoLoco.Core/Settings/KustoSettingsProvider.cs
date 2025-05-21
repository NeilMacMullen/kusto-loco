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

    private Stack<ImmutableDictionary<string, RawKustoSetting>> _settingsStack;

    public KustoSettingsProvider()
    {
        _settingsStack = new Stack<ImmutableDictionary<string, RawKustoSetting>>();
        _settingsStack.Push(ImmutableDictionary<string, RawKustoSetting>.Empty);
    }

    public void AddLayer(KustoSettingsProvider layeredSettings) => Push(layeredSettings.GetFlattened());

    public IReadOnlyCollection<KustoSettingDefinition> GetDefinitions() => _registeredSettings;

    public void Register(params KustoSettingDefinition[] settings)
    {
        foreach (var setting in settings)
            _registeredSettings = _registeredSettings.Add(setting);
    }

    public void Push(ImmutableDictionary<string, RawKustoSetting> layer) => _settingsStack.Push(layer);

    public ImmutableDictionary<string, RawKustoSetting> Pop()
    {
        if (_settingsStack.Any())
            return _settingsStack.Pop();
        return ImmutableDictionary<string, RawKustoSetting>.Empty;
    }

    public void Set(string setting, string value)
    {
        var k = new RawKustoSetting(setting, value);

        var all = _settingsStack.Reverse().ToArray();
        var last = all[0];
        last = last.SetItem(k.Key, k);
        all[0] = last;
        _settingsStack = new Stack<ImmutableDictionary<string, RawKustoSetting>>(all);
    }

    public void Set(string setting, bool value) => Set(setting, value.ToString());

    public void Set(string setting, int value) => Set(setting, value.ToString());


    private ImmutableDictionary<string, RawKustoSetting> GetFlattened()
    {
        var layers = _settingsStack.ToArray();
        var d = new Dictionary<string, RawKustoSetting>();
        foreach (var layer in layers)
        foreach (var s in layer.Values)
            d.TryAdd(s.Key, s);

        return d.ToImmutableDictionary();
    }

    public string Get(KustoSettingDefinition setting)
    {
        var settingName = setting.Name;
        var fb = new RawKustoSetting(settingName, setting.DefaultValue);
        var settings = GetFlattened();
        return settings.GetValueOrDefault(settingName.ToLowerInvariant(), fb).Value;
    }

    /// <summary>
    ///     Tries to interpret a string as a setting name
    /// </summary>
    /// <remarks>
    ///     There are a number of places in application code where it's convenient to see if the
    ///     supplied string might actually be interpreted as a setting name.  For example
    ///     .set abc xyz
    ///     .command abc
    ///     --> could transform to .command xyz
    /// </remarks>
    public string TrySubstitute(string name)
    {
        var fb = new RawKustoSetting(name, name);
        var settings = GetFlattened();
        return settings.GetValueOrDefault(name.ToLowerInvariant(), fb).Value;
    }

    public bool HasSetting(string name)
        => GetFlattened().ContainsKey(name.ToLowerInvariant());

    public string GetOr(string setting, string fallback)
    {
        var fb = new KustoSettingDefinition(setting, string.Empty, fallback, string.Empty);
        return Get(fb);
    }

    /// <summary>
    ///     Try to fetch a setting and interpret it as a number (int)
    /// </summary>
    /// <remarks> return the fallback value if the number can't be parsed</remarks>
    public int GetIntOr(string setting, int fallback)
    {
        var s = GetOr(setting, fallback.ToString());
        return int.TryParse(s, out var v) ? v : fallback;
    }

    /// <summary>
    ///     Try to interpret the setting as a boolean
    /// </summary>
    /// <remarks>
    ///     Try using true/false first, then yes/no or use non-zero numeric value to indicate true.
    ///     A missing setting will be interpreted as false.
    /// </remarks>
    public bool GetBool(KustoSettingDefinition setting)
    {
        var trueValues = new[] { "true", "yes", "on", "1" };
        var falseValues = new[] { "false", "no", "off", "0" };
        var s = Get(setting).ToLowerInvariant();
        if (trueValues.Contains(s)) return true;
        //if (falseValues.Contains(s)) return false;
        return false;
    }

    public IEnumerable<RawKustoSetting> Enumerate() => GetFlattened().Values;

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
        _settingsStack = new Stack<ImmutableDictionary<string, RawKustoSetting>>();
        AddLayer(new KustoSettingsProvider());
    }

    public double GetDoubleOr(string settingName, double p1)
    {
        var s = GetOr(settingName, "");
        return double.TryParse(s, out var v) ? v : p1;
    }

    /// <summary>
    ///     Create a snapshot of the current settings
    /// </summary>
    /// <returns></returns>
    public KustoSettingsProvider Snapshot()
    {
        var settings = GetFlattened();
        var newSettings = new KustoSettingsProvider();
        newSettings.Push(settings);
        return newSettings;
    }
}
