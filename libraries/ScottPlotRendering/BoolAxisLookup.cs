namespace KustoLoco.Rendering.ScottPlot;

public class BoolAxisLookup : IAxisLookup
{
    private static readonly Dictionary<double, string> _axisValuesAndLabels = new()
    {
        { 1, "False" },
        { 2, "True" },
    };
    public double AxisValueFor(object? o) => o is bool b
        ? b
            ? 2
            : 1
        : 0;

    //we shouldn't need to generate labels for a numeric axis
    public Dictionary<double, string> AxisValuesAndLabels() => _axisValuesAndLabels;
}