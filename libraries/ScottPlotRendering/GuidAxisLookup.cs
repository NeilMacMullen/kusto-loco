namespace KustoLoco.ScottPlotRendering;

public class GuidAxisLookup : IAxisLookup
{
    private readonly StringAxisLookup _stringAxis;

    public GuidAxisLookup(object?[] data)
    {
        _stringAxis = new StringAxisLookup(data.Select(d=>d is Guid g ? g.ToString() : null).ToArray<object?>());
    }
    public double AxisValueFor(object? o) => o is Guid g 
        ? _stringAxis.AxisValueFor(g.ToString())
        : 0;

    //we shouldn't need to generate labels for a numeric axis
    public Dictionary<double, string> AxisValuesAndLabels() => _stringAxis.AxisValuesAndLabels();
}
