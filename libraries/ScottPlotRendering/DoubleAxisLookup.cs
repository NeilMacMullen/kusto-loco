namespace KustoLoco.ScottPlotRendering;

public class DoubleAxisLookup : IAxisLookup
{
    public double ValueFor(object? o) => o is null ? 0 : (double)o;
    public string GetLabel(double position) => throw new NotImplementedException();
    public Dictionary<double, string> Dict() => throw new NotImplementedException();
}