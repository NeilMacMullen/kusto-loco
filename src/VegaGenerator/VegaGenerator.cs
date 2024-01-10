using Extensions;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public static class VegaGenerator
{
    public const string AreaChart = "area";
    public const string LineChart = "line";
    public const string AxisTypeTime = "temporal";
    public const string AxisTypeQuantity = "quantitative";
    public const string AxisTypeOrdinal = "ordinal";
    public const string AxisTypeNominal = "nominal";
    public const string BarChart = "bar";
    public const string PieChart = "arc";
    public const string GridChart = "rect";


    public static VegaChart Spec(
        string chartType,
        string xAxisType,
        string yAxisType,
        ColumnAndName xSeries,
        ColumnAndName ySeries,
        ColumnAndName colorSeries
    )
    {
        var encoding = new VegaEncoding
                       {
                           x = new VegaSeries
                               {
                                   field = xSeries.QualifiedColumnName,
                                   type = xAxisType,
                                   axis = new VegaAxis
                                          { title = xSeries.Text, minExtent = 0 },
                                   title = xSeries.Text,
                               },
                           y = new VegaSeries
                               {
                                   field = ySeries.QualifiedColumnName,
                                   type = yAxisType,
                                   axis = new VegaAxis
                                          { title = ySeries.Text, minExtent = 60 },
                                   title = ySeries.Text,
                               }
                       }
            ;
        if (colorSeries.QualifiedColumnName.IsNotBlank())
            encoding.color = new VegaColorDefinition
                             {
                                 field = colorSeries.QualifiedColumnName,
                                 type = AxisTypeNominal,
                                 title = colorSeries.Text,
                             };

        var chart = new VegaChart(
                                  chartType,
                                  encoding
                                 );
        return chart;
    }

    public static VegaTransform CreateCumulativeRanking(string sourceData, string outputName)
    {
        return new VegaTransform
               {
                   sort =
                   [
                       new VegaSeries
                       {
                           field = sourceData,
                           axis = new VegaAxis()
                       }
                   ],
                   window =
                   [
                       new VegaWindow
                       {
                           op = "percent_rank",
                           field = "count",
                           @as = outputName
                       }
                   ],
                   frame = new object?[] { null, 0 }
               };
    }

    public class VegaAxis
    {
        public string title { get; set; } = string.Empty;
        public int minExtent { get; set; }
    }

    public class VegaField
    {
        public string field { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
    }

    public class VegaSeries : VegaField
    {
        public VegaAxis axis { get; set; } = new();
        public bool bin { get; set; }
        public string title { get; set; }
    }

    public class VegaEncoding
    {
        public VegaSeries? x { get; set; }
        public VegaSeries? x2 { get; set; }
        public VegaSeries? y { get; set; }

        /// <summary>
        ///     used for pie charts
        /// </summary>
        /// [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public VegaField? theta { get; set; }

        public VegaColorDefinition? color { get; set; }
    }

    public class VegaLegend
    {
        public string orient { get; set; } = "bottom-left";
        public bool disable { get; set; }
    }


    public record VegaMark
    {
        public string type { get; set; } = LineChart;

        public double? width { get; set; }

        /// <summary>
        ///     Turns on tooltips
        /// </summary>
        /// <remarks>
        ///     In the future it may be better to support "nearest" or even cursor based tooltips.
        ///     See https://stackoverflow.com/questions/74796097/vega-lite-line-mark-show-tooltip-at-a-distance
        ///     and https://vega.github.io/vega-lite/examples/interactive_multi_line_pivot_tooltip.html
        /// </remarks>
        public bool tooltip { get; set; } = true;
    }

    public class VegaChart
    {
        public VegaChart(string mark, VegaEncoding encoding)
        {
            this.mark = this.mark with { type = mark };
            this.encoding = encoding;
        }

        public string title { get; set; } = string.Empty;
        public VegaMark mark { get; set; } = new();
        public VegaEncoding encoding { get; set; }
        public VegaTransform[] transform { get; set; } = Array.Empty<VegaTransform>();
        public VegaConfig? config { get; set; } = new();
    }

    public class VegaConfig
    {
        public VegaLegend? legend { get; set; } = new();
    }

    public class VegaColorDefinition : VegaField
    {
        public VegaLegend? Legend = null;
        public string title { get; set; } = string.Empty;
    }

    public class VegaTransform
    {
        public VegaSeries[] sort { get; set; } = Array.Empty<VegaSeries>();
        public VegaWindow[] window { get; set; } = Array.Empty<VegaWindow>();
        public object?[] frame { get; set; } = Array.Empty<object>();
    }

    public class VegaWindow
    {
        public string op { get; set; }
        public string field { get; set; }
        public string @as { get; set; }
    }
}
