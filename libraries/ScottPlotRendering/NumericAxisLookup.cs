using System.Numerics;

namespace KustoLoco.Rendering.ScottPlot;

public class NumericAxisLookup<T> : IAxisLookup
    where T : INumber<T>
{
    public double AxisValueFor(object? o) => o is T t ?  Convert.ToDouble(t) :0;

    //we shouldn't need to generate labels for a numeric axis
    public Dictionary<double, string> AxisValuesAndLabels() => throw new NotImplementedException();
}
