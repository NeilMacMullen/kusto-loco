namespace KustoLoco.ScottPlotRendering;

public class DateTimeAxisLookup : IAxisLookup
{
    public double AxisValueFor(object? o) => o is null ? 0 : ((DateTime)o).ToOADate();
   
    public Dictionary<double, string> AxisValuesAndLabels() => throw new NotImplementedException();
}
