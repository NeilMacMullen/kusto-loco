namespace KustoLoco.ScottPlotRendering;

public interface IAxisLookup
{
    public double ValueFor(object? o);
    string GetLabel(double position);
    Dictionary<double, string> Dict();
}