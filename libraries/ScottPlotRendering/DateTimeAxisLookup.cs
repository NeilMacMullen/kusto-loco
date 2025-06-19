namespace KustoLoco.Rendering.ScottPlot;

public class DateTimeAxisLookup : IAxisLookup
{
    public double AxisValueFor(object? o) => ((DateTime?)o)?.ToOADate() ?? 0;
   
    public Dictionary<double, string> AxisValuesAndLabels() => throw new NotImplementedException();
}
