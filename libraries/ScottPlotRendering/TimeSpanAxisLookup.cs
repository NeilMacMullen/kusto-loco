namespace KustoLoco.Rendering.ScottPlot;

public class TimeSpanAxisLookup : IAxisLookup
{
   
    public double AxisValueFor(object? o) => o is TimeSpan b
        ? b.TotalSeconds
        : 0;

    //we shouldn't need to generate labels for a numeric axis
    public Dictionary<double, string> AxisValuesAndLabels() => throw new NotImplementedException();
}