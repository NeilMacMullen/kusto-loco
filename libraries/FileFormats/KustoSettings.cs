using System.Collections.Immutable;

namespace KustoLoco.FileFormats;



public record RawKustoSetting(string Name, string Value)
{
    public string Key => Name.ToLowerInvariant();
}
/// <summary>
///     Provides a flexible way of passing settings around the various KustoLoco layers
/// </summary>
/// <remarks>
///     Yes, it's another string/string dictionary with basic type conversion.
///     The benefit of loose typing here is that it's easy to add new settings
///     without changing the interface and it fits well with the idea of allowing the user
///     to change behaviour by typing in values dynamically.  For example, a user in lokqldx could
///     type in "set csv.inferColumnTypes false" to disable automatic type inference.
///     Settings are _mutable_ because we don't want to pass them in for every operation.
///     However. it would be good to find a way to get back to immutability.
/// </remarks>
public class KustoSettings
{
    private ImmutableDictionary<string, RawKustoSetting> _settings = ImmutableDictionary<string, RawKustoSetting>.Empty;

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


    public string Get(string setting, string fallbackValue)
    {
        var fb = new RawKustoSetting(setting, fallbackValue);
       return _settings.GetValueOrDefault(setting.ToLowerInvariant(), fb).Value;
    }


    public int Get(string setting, int fallbackValue)
    {
        var s = Get(setting, string.Empty);
        return int.TryParse(s, out var v) ? v : fallbackValue;
    }

    /// <summary>
    ///     Try to interpret the setting as a boolean
    /// </summary>
    /// <remarks>
    ///     Try using true/false first, then yes/no or use non-zero numeric value to indicate true
    /// </remarks>
    public bool Get(string setting, bool fallbackValue)
    {
        var s = Get(setting, string.Empty);
        if (bool.TryParse(s, out var b))
            return b;
        if (s.Equals("yes", StringComparison.OrdinalIgnoreCase))
            return true;
        if (s.Equals("no", StringComparison.OrdinalIgnoreCase))
            return false;
        if (int.TryParse(s, out var v))
            return v > 0;
        return fallbackValue;
    }

    public IEnumerable<RawKustoSetting> Enumerate() => _settings.Values;
}
