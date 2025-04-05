using KustoLoco.Core;

namespace KustoLoco.ScottPlotRendering;

public class AxisLookup
{
    public static IAxisLookup From(ColumnResult col, object?[] data) =>
        col.UnderlyingType switch
        {
            { } type when type == typeof(double) => new NumericAxisLookup<double>(),
            { } type when type == typeof(long) => new NumericAxisLookup<long>(),
            { } type when type == typeof(float) => new NumericAxisLookup<float>(),
            { } type when type == typeof(int) => new NumericAxisLookup<int>(),
            { } type when type == typeof(short) => new NumericAxisLookup<short>(),
            { } type when type == typeof(string) => new StringAxisLookup(data),
            { } type when type == typeof(DateTime) => new DateTimeAxisLookup(),
            { } type when type == typeof(bool) => new BoolAxisLookup(),
            { } type when type == typeof(Guid) => new GuidAxisLookup(data),
            { } type when type == typeof(TimeSpan) => new TimeSpanAxisLookup(),

            _ => throw new InvalidOperationException($"Unsupported type {col.UnderlyingType.Name}")
        };
}
