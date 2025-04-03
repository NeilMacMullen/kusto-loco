using KustoLoco.Core;

namespace KustoLoco.ScottPlotRendering;

public class AxisLookup
{
    public static IAxisLookup From(ColumnResult col, object?[] data)
    {
        if (col.UnderlyingType == typeof(string))
        {
            var d = new Dictionary<object, double>();
            var index = 1.0;
            foreach (var o in data)
            {
                if (o is null)
                    continue;
                if (!d.ContainsKey(o))
                    d[o] = index++;
            }

            return new StringAxisLookup(d);
        }

        if (col.UnderlyingType == typeof(DateTime)) return new DateTimeAxisLookup();

        if (col.UnderlyingType == typeof(double)) return new DoubleAxisLookup();

        if (col.UnderlyingType == typeof(long)) return new LongAxisLookup();

        throw new InvalidOperationException($"Unsupported type {col.UnderlyingType.Name}");
    }
}