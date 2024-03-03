using System.Collections.Specialized;
using JPoke;

#pragma warning disable CS8618
public class VegaChart
{
    public JObjectBuilder _builder;

    public VegaChart() => _builder = JObjectBuilder.CreateEmpty();

    private string ToVegaString(object o)
    {
        if (o == null) throw new ArgumentNullException(nameof(o));
        return o.ToString()!.ToLowerInvariant().Replace("_", "-");
    }

    private string Axis(VegaAxisName name) => $"encoding.{ToVegaString(name)}";

    public VegaChart AddSeries(VegaAxisName axisName, ColumnDescription column)
    {
        var axis = Axis(axisName);
        _builder.Set($"{axis}.axis.title", column.Text);
        _builder.Set($"{axis}.field", column.QualifiedColumnName);
        _builder.Set($"{axis}.type", ToVegaString(column.VegaAxisType));
        return this;
    }

    public VegaChart SetMark(VegaMark mark)
    {
        _builder.Set("mark.type", ToVegaString(mark));
        _builder.Set("mark.tooltip", true);
        return this;
    }

    public VegaChart SetTitle(string title)
    {
        _builder.Set("title", title);
        return this;
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

    private VegaChart RenameAxis(VegaAxisName source, VegaAxisName target)
    {
        _builder.Move(Axis(source), Axis(target));
        return this;
    }

    private VegaChart CopyAxis(VegaAxisName source, VegaAxisName target)
    {
        _builder.Copy(Axis(source), Axis(target));
        return this;
    }


    private VegaChart ConvertToPie()
        => RenameAxis(VegaAxisName.X, VegaAxisName.Color)
            .RenameAxis(VegaAxisName.Y, VegaAxisName.Theta);

    public VegaChart ConvertToTimeline()
        => RenameAxis(VegaAxisName.Y, VegaAxisName.X2)
            .CopyAxis(VegaAxisName.Color, VegaAxisName.Y)
            .DisableLegend();

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

    public VegaChart InjectData(IEnumerable<OrderedDictionary> rows)
    {
        _builder.Set("data.values", rows);
        return this;
    }

    public VegaChart UseDataSource(string name)
    {
        _builder.Set("data.name", name);
        return this;
    }

    public VegaChart FillContainer()
    {
        _builder.Set("width", "container");
        _builder.Set("height", "container");
        return this;
    }

    public VegaChart SetSize(int width, int height)
    {
        _builder.Set("width", width);
        _builder.Set("height", height);
        return this;
    }

    public string Serialize() => _builder.Serialize();

    public void SetLegendPosition(VegaLegendPosition pos)
    {
        _builder.Set("config.legend.orient", ToVegaString(pos));
    }

    public VegaChart MakeCumulativePercent(string sourceData, string outputName)
    {
        _builder.Set("transform[0].sort[0]", new { field = sourceData });
        _builder.Set("transform[0].window[0]", new
        {
            op = "percent_rank",
            field = "count",
            @as = outputName
        });
        _builder.Set("transform[0].frame", new object?[] { null, 0 });

        _builder.Set($"{Axis(VegaAxisName.Y)}.field", outputName);

        return this;
    }

    public VegaChart UseCursorTooltip(string[] seriesNames)
    {
        var xAxisField = _builder.Get($"{Axis(VegaAxisName.X)}.field", string.Empty);
        var yAxisField = _builder.Get($"{Axis(VegaAxisName.Y)}.field", string.Empty);
        var colorAxisField = _builder.Get($"{Axis(VegaAxisName.Color)}.field", string.Empty);

        var toolTips = seriesNames.Select(s => new
            {
                field = s,
                type = ToVegaString(VegaAxisType.Quantitative)
            })
            .ToArray();


        var rulerLayer = $$$"""
                            [
                            {
                             "layer": [
                                {"mark": "line"},
                                {"transform": [{"filter": {"param": "hover", "empty": false}}], "mark": "point"}
                              ]
                            },
                            {
                              "transform": [{"pivot": "{{{colorAxisField}}}", "value": "{{{yAxisField}}}", "groupby": ["{{{xAxisField}}}"]}],
                              "mark": "rule",
                              "encoding": {
                                "opacity": {
                                  "condition": {"value": 0.3, "param": "hover", "empty": false},
                                  "value": 0
                                }
                              },
                              "params": [{
                                "name": "hover",
                                "select": {
                                  "type": "point",
                                  "fields": ["{{{xAxisField}}}"],
                                  "nearest": true,
                                  "on": "pointerover",
                                  "clear": "pointerout"
                                }
                              }]
                            }
                            ]
                            """;
        var layerBuilder = JObjectBuilder.FromJsonText(rulerLayer);
        _builder.Set("layer", layerBuilder);
        _builder.Set("layer[1].encoding.tooltip", toolTips);
        _builder.Move($"{Axis(VegaAxisName.Y)}", $"layer[0].{Axis(VegaAxisName.Y)}");
        return this;
    }

    public VegaChart UseCursorTooltip()
    {
        var xAxisField = _builder.Get($"{Axis(VegaAxisName.X)}.field", string.Empty);
        var yAxisField = _builder.Get($"{Axis(VegaAxisName.Y)}.field", string.Empty);
        var colorAxisField = _builder.Get($"{Axis(VegaAxisName.Color)}.field", string.Empty);

        //note to cope with spaces and invalid chars in the x-axis name we use bracket rather than dot notation
        //to embed it in the calculate expression
        var rulerLayer = $$$"""
                            [
                            {
                             "layer": [
                                {"mark": "line"},
                                {"transform": [{"filter": {"param": "hover", "empty": false}}], "mark": "point"}
                              ]
                            },
                            {
                              "transform": [
                              {"pivot": "{{{colorAxisField}}}", "value": "{{{yAxisField}}}", "groupby": ["{{{xAxisField}}}"]},
                                {"calculate": "datetime(datum['{{{xAxisField}}}'])", "as": "{{{xAxisField}}}"}
                            
                              ],
                              "mark": { "type" : "rule",
                                   "tooltip": {"content": "data"}
                                     },
                              "encoding": {
                                "opacity": {
                                  "condition": {"value": 0.3, "param": "hover", "empty": false},
                                  "value": 0
                                }
                              },
                              "params": [{
                                "name": "hover",
                                "select": {
                                  "type": "point",
                                  "fields": ["{{{xAxisField}}}"],
                                  "nearest": true,
                                  "on": "pointerover",
                                  "clear": "pointerout"
                                }
                              }]
                            }
                            ]
                            """;
        var layerBuilder = JObjectBuilder.FromJsonText(rulerLayer);
        _builder.Set("layer", layerBuilder);
        _builder.Move($"{Axis(VegaAxisName.Y)}", $"layer[0].{Axis(VegaAxisName.Y)}");
        return this;
    }


    public VegaChart AddFacet(ColumnDescription column)
    {
        _builder.Set("encoding.facet", new
        {
            field = column.QualifiedColumnName,
            columns = 1
        });

        return this;
    }
}