namespace KustoLoco.ScottPlotRendering;

public class DateTimeAxisLookup : IAxisLookup
{
    public double ValueFor(object? o) => o is null ? 0 : ((DateTime)o).ToOADate();
    public string GetLabel(double position) => throw new NotImplementedException();
    public Dictionary<double, string> Dict() => throw new NotImplementedException();
}