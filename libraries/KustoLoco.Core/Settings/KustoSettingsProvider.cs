using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NotNullStrings;

namespace KustoLoco.Core.Settings;

/// <summary>
/// Provides a flexible, layered settings provider for KustoLoco components.
/// </summary>
/// <remarks>
/// This class manages settings as string key-value pairs with basic type conversion,
/// supporting dynamic and mutable configuration. Settings can be layered, allowing
/// overrides and snapshots. Designed for scenarios where users or components may
/// dynamically adjust behavior at runtime.
/// </remarks>
public class KustoSettingsProvider
{
    private ImmutableHashSet<KustoSettingDefinition> _registeredSettings =
        ImmutableHashSet<KustoSettingDefinition>.Empty;

    private Stack<ImmutableDictionary<string, RawKustoSetting>> _settingsStack;

    /// <summary>
    /// Initializes a new instance of the <see cref="KustoSettingsProvider"/> class.
    /// </summary>
    public KustoSettingsProvider()
    {
        _settingsStack = new Stack<ImmutableDictionary<string, RawKustoSetting>>();
        _settingsStack.Push(ImmutableDictionary<string, RawKustoSetting>.Empty);
    }

    /// <summary>
    /// Adds a new layer of settings from another <see cref="KustoSettingsProvider"/>.
    /// </summary>
    /// <remarks>
    ///  Used primarily by the macro system to push parameters as settings during the
    /// execution
    /// </remarks>
    /// <param name="layeredSettings">The provider whose settings will be layered on top.</param>
    public void AddLayer(KustoSettingsProvider layeredSettings)
        => _settingsStack
        .Push(layeredSettings.GetFlattened());

    /// <summary>
    /// Gets the collection of registered setting definitions.
    /// </summary>
    /// <returns>A read-only collection of <see cref="KustoSettingDefinition"/>.</returns>
    public IReadOnlyCollection<KustoSettingDefinition> GetDefinitions() => _registeredSettings;

    /// <summary>
    /// Registers one or more setting definitions.
    /// </summary>
    /// <param name="settings">The settings to register.</param>
    public void Register(params KustoSettingDefinition[] settings)
    {
        foreach (var setting in settings)
            _registeredSettings = _registeredSettings.Add(setting);
    }

   

    /// <summary>
    /// Pops the top settings layer from the stack.
    /// </summary>
    /// <returns>The popped settings layer, or an empty dictionary if the stack is empty.</returns>
    public ImmutableDictionary<string, RawKustoSetting> Pop() =>
        _settingsStack.Any()
            ? _settingsStack.Pop()
            : ImmutableDictionary<string, RawKustoSetting>.Empty;

    /// <summary>
    /// Sets a string value for a setting.
    /// </summary>
    /// <param name="setting">The setting name.</param>
    /// <param name="value">The value to set.</param>
    public void Set(string setting, string value)
    {
        var k = new RawKustoSetting(setting, value);

        var all = _settingsStack.Reverse().ToArray();
        var last = all[0];
        last = last.SetItem(k.Key, k);
        all[0] = last;
        _settingsStack = new Stack<ImmutableDictionary<string, RawKustoSetting>>(all);
    }

    /// <summary>
    /// Sets a boolean value for a setting.
    /// </summary>
    /// <param name="setting">The setting name.</param>
    /// <param name="value">The boolean value to set.</param>
    public void Set(string setting, bool value) => Set(setting, value.ToString());

    /// <summary>
    /// Sets an integer value for a setting.
    /// </summary>
    /// <param name="setting">The setting name.</param>
    /// <param name="value">The integer value to set.</param>
    public void Set(string setting, int value) => Set(setting, value.ToString());

    /// <summary>
    /// Flattens all settings layers into a single dictionary.
    /// </summary>
    /// <returns>An immutable dictionary of all settings.</returns>
    private ImmutableDictionary<string, RawKustoSetting> GetFlattened()
    {
        var layers = _settingsStack.ToArray();
        var d = new Dictionary<string, RawKustoSetting>();
        foreach (var layer in layers)
            foreach (var s in layer.Values)
                d.TryAdd(s.Key, s);

        return d.ToImmutableDictionary();
    }

    /// <summary>
    /// Gets the value of a setting, or its default if not set.
    /// </summary>
    /// <param name="setting">The setting definition.</param>
    /// <returns>The setting value as a string.</returns>
    public string Get(KustoSettingDefinition setting)
    {
        var settingName = setting.Name;
        var fb = new RawKustoSetting(settingName, setting.DefaultValue);
        var settings = GetFlattened();
        return settings.GetValueOrDefault(settingName.ToLowerInvariant(), fb).Value;
    }

    /// <summary>
    /// Tries to interpret a string as a setting name and substitute its value.
    /// </summary>
    /// <param name="name">The potential setting name.</param>
    /// <returns>The setting value if found; otherwise, returns the input name.</returns>
    public string TrySubstitute(string name)
    {
        var fb = new RawKustoSetting(name, name);
        var settings = GetFlattened();
        return settings.GetValueOrDefault(name.ToLowerInvariant(), fb).Value;
    }

    /// <summary>
    /// Determines whether a setting with the specified name exists.
    /// </summary>
    /// <param name="name">The setting name.</param>
    /// <returns><c>true</c> if the setting exists; otherwise, <c>false</c>.</returns>
    public bool HasSetting(string name)
        => GetFlattened().ContainsKey(name.ToLowerInvariant());

    /// <summary>
    /// Gets the value of a setting or a fallback value if not set.
    /// </summary>
    /// <param name="setting">The setting name.</param>
    /// <param name="fallback">The fallback value.</param>
    /// <returns>The setting value or the fallback.</returns>
    public string GetOr(string setting, string fallback)
    {
        var fb = new KustoSettingDefinition(setting, string.Empty, fallback, string.Empty);
        return Get(fb);
    }

    /// <summary>
    /// Gets the value of a setting as an integer, or a fallback if not set or invalid.
    /// </summary>
    /// <param name="setting">The setting name.</param>
    /// <param name="fallback">The fallback integer value.</param>
    /// <returns>The setting value as an integer, or the fallback.</returns>
    public int GetIntOr(string setting, int fallback)
    {
        var s = GetOr(setting, fallback.ToString());
        return int.TryParse(s, out var v) ? v : fallback;
    }

    /// <summary>
    /// Gets the value of a setting as a boolean.
    /// </summary>
    /// <param name="setting">The setting definition.</param>
    /// <returns><c>true</c> if the setting is interpreted as true; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Accepts "true", "yes", "on", "1" as true; "false", "no", "off", "0" as false.
    /// Missing or unrecognized values are interpreted as false.
    /// </remarks>
    public bool GetBool(KustoSettingDefinition setting)
    {
        var trueValues = new[] { "true", "yes", "on", "1" };
        var falseValues = new[] { "false", "no", "off", "0" };
        var s = Get(setting).ToLowerInvariant();
        return trueValues.Contains(s);
    }

    /// <summary>
    /// Enumerates all current settings as <see cref="RawKustoSetting"/> values.
    /// </summary>
    /// <returns>An enumerable of all settings.</returns>
    public IEnumerable<RawKustoSetting> Enumerate() => GetFlattened().Values;

    /// <summary>
    /// Gets a setting as an array of path strings, split by ';'.
    /// </summary>
    /// <param name="name">The setting definition.</param>
    /// <returns>An array of path strings.</returns>
    /// <remarks>
    /// Path-lists use the ';' separator and elements are trimmed.
    /// </remarks>
    public string[] GetPathList(KustoSettingDefinition name)
    {
        var k = Get(name);
        return k.Tokenize(";");
    }

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
    public void Reset()
    {
        _settingsStack = new Stack<ImmutableDictionary<string, RawKustoSetting>>();
        AddLayer(new KustoSettingsProvider());
    }

    /// <summary>
    /// Gets the value of a setting as a double, or a fallback if not set or invalid.
    /// </summary>
    /// <param name="settingName">The setting name.</param>
    /// <param name="p1">The fallback double value.</param>
    /// <returns>The setting value as a double, or the fallback.</returns>
    public double GetDoubleOr(string settingName, double p1)
    {
        var s = GetOr(settingName, "");
        return double.TryParse(s, out var v) ? v : p1;
    }

    /// <summary>
    /// Creates a snapshot of the current settings
    /// </summary>
    /// <remarks>
    /// The snapshot contains a completely independent set of values to the original
    /// </remarks>
    /// <returns>A new <see cref="KustoSettingsProvider"/> containing the current settings.</returns>
    public KustoSettingsProvider Snapshot()
    {
        var newSettings = new KustoSettingsProvider();
        newSettings.AddLayer(this);
        return newSettings;
    }
}
