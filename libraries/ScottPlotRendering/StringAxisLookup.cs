using NotNullStrings;

namespace KustoLoco.ScottPlotRendering;

public class StringAxisLookup : IAxisLookup
{
    private readonly Dictionary<double, object> _labelLookup;
    private readonly Dictionary<object, double> _lookup;

    public StringAxisLookup(Dictionary<object, double> lookup)
    {
        _lookup = lookup;
        _labelLookup = _lookup.ToDictionary(kv => kv.Value, kv => kv.Key);
    }

    public double ValueFor(object? o) => o is null ? 0 : _lookup[o];

    public string GetLabel(double position) => _labelLookup.TryGetValue(position, out var o)
        ? o.ToString().NullToEmpty()
        : string.Empty;

    public Dictionary<double, string> Dict() =>
        _labelLookup.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString().NullToEmpty()!);
}
