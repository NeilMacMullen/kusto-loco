using NotNullStrings;

namespace KustoLoco.Rendering.ScottPlot;

/// <summary>
/// Axis Lookup for string values
/// </summary>
/// <remarks>
/// Sometimes we just want to plot strings on the axis.
/// </remarks>
public class StringAxisLookup : IAxisLookup
{
    private readonly Dictionary<double, string> _labelLookup;
    private readonly Dictionary<string, double> _lookup;

    public StringAxisLookup(object? [] data)
    {
        _lookup = new Dictionary<string, double>();
        //start at 1.0 to reserve 0.0 for null
        var index = 1.0;
        foreach (var o in data)
        {
            if (o is not string s)
                continue;
            if (!_lookup.ContainsKey(s))
                _lookup[s] = index++;
        }
        _labelLookup = _lookup.ToDictionary(kv => kv.Value, kv => kv.Key);
    }

    public double AxisValueFor(object? o) => o is string s ? _lookup[s] : 0.0;


    public Dictionary<double, string> AxisValuesAndLabels() =>
        _labelLookup;
}
