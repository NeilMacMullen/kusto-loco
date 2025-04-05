namespace KustoLoco.ScottPlotRendering;

/// <summary>
/// Interface for converting axis object values to doubles used for plotting.
/// </summary>
public interface IAxisLookup
{
    /// <summary>
    /// Return the numeric position of a logical axis value
    /// </summary>
    public double AxisValueFor(object? o);

    /// <summary>
    /// Returns a dictionary mapping double values to their corresponding string labels for axis representation.
    /// </summary>
    Dictionary<double, string> AxisValuesAndLabels();
}


