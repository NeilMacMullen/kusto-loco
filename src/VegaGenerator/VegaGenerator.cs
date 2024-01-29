using System.Collections.Specialized;
using JPoke;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


public class VegaChart
{
    public JObjectBuilder _builder;

    public VegaChart() => _builder = JObjectBuilder.CreateEmpty();

    private string ToVegaString(object o)
    {
        if (o == null) throw new ArgumentNullException(nameof(o));
        return o.ToString()!.ToLowerInvariant();
    }

    private string Axis(VegaAxisName name) => $"encoding.{ToVegaString(name)}";

    public void AddSeries(VegaAxisName axisName, ColumnDescription column)
    {
        var axis = Axis(axisName);
        _builder.Set($"{axis}.axis.title", column.Text);
        _builder.Set($"{axis}.field", column.QualifiedColumnName);
        _builder.Set($"{axis}.type", ToVegaString(column.VegaAxisType));
    }

    public void SetMark(VegaMark mark)
    {
        _builder.Set("mark.type", ToVegaString(mark));
        _builder.Set("mark.tooltip", true);
    }

    public void SetTitle(string title)
    {
        _builder.Set("title", title);
    }

    public static VegaChart CreateVegaChart(
        VegaMark chartType,
        ColumnDescription xSeries,
        ColumnDescription ySeries,
        ColumnDescription colorSeries
    )
    {
        var chart = new VegaChart();
        chart.SetMark(chartType);
        chart.AddSeries(VegaAxisName.X, xSeries);
        chart.AddSeries(VegaAxisName.Y, ySeries);
        if (colorSeries.QualifiedColumnName.Length != 0)
            chart.AddSeries(VegaAxisName.Color, colorSeries);

        if (chartType == VegaMark.Arc)
        {
            chart.ConvertToPie();
        }

        return chart;
    }

    private void RenameAxis(VegaAxisName source, VegaAxisName target)
        => _builder.Move(Axis(source), Axis(target));

    private void ConvertToPie()
    {
        RenameAxis(VegaAxisName.X, VegaAxisName.Color);
        RenameAxis(VegaAxisName.Y, VegaAxisName.Theta);
    }

    public void ConvertToTimeline()
    {
        RenameAxis(VegaAxisName.Y, VegaAxisName.X2);
        RenameAxis(VegaAxisName.Color, VegaAxisName.Y);
        DisableLegend();
    }

    private VegaChart DisableLegend()
    {
        _builder.Set("config.legend.disable", true);
        return this;
    }

    public VegaChart StackAxis(VegaAxisName axis)
    {
        _builder.Set($"{Axis(axis)}.aggregate", "sum");
        return this;
    }

    public static VegaAxisType InferSuitableAxisType(Type t)
    {
        var numericTypes = new[]
        {
            typeof(int), typeof(long), typeof(double), typeof(float),
            typeof(uint), typeof(ulong), typeof(byte), typeof(decimal)
        };
        if (numericTypes.Contains(t))
            return VegaAxisType.Quantitative;

        if (t == typeof(TimeSpan))
            return VegaAxisType.Ordinal;

        return t == typeof(DateTime)
            ? VegaAxisType.Temporal
            : VegaAxisType.Nominal;
    }

    public void InjectData(IEnumerable<OrderedDictionary> rows)
    {
        _builder.Set("data.values", rows);
    }

    public void FillContainer()
    {
        _builder.Set("width", "container");
        _builder.Set("height", "container");
    }

    public string Serialize() => _builder.Serialize();
}

public enum VegaMark
{
    Area,
    Line,
    Bar,
    Arc,
    Grid,
    Point
}

public enum VegaAxisType
{
    Temporal,
    Quantitative,
    Ordinal,
    Nominal
}

public enum VegaAxisName
{
    X,
    Y,
    X2,
    Theta,
    Color
}