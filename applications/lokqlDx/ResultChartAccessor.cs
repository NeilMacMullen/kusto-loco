using KustoLoco.Core;
using NotNullStrings;
using ScottPlot;
using static MoreLinq.Extensions.PairwiseExtension;
namespace lokqlDx;

public class ResultChartAccessor
{
    private readonly KustoQueryResult _result = KustoQueryResult.Empty;
    private ColumnResult _xColumn;
    private ColumnResult _seriesNameColumn;
    private IAxisLookup _xLookup = new DoubleAxisLookup();
    private ColumnResult _valueColumn;
    private IAxisLookup _valueLookup=new DoubleAxisLookup();

    public ResultChartAccessor(KustoQueryResult result)
    {
        _result = result;

    }

    public enum ChartKind
    {
        Table,
        Line,
        Bar,
        Pie,
        Column,
        Scatter,
        Ladder
    }

    public ChartKind Kind()
    {
        var k = _result.Visualization.ChartType.ToLowerInvariant();
        return k switch
        {
            "linechart" => ChartKind.Line,
            "barchart" => ChartKind.Bar,
            "scatterchart" => ChartKind.Scatter,
            "ladderchart" => ChartKind.Ladder,
            "piechart" => ChartKind.Pie,
            "columnchart" => ChartKind.Column,
            _ => ChartKind.Table
        };
    }

    private IAxisLookup CreateLookup(ColumnResult column)
    {
        var fullData = _result.EnumerateColumnData(column).ToArray();
        return AxisLookup.From(column, fullData);
    }

    public void AssignXColumn(int i)
    {
        _xColumn = _result.ColumnDefinitions()[i];
        _xLookup = CreateLookup(_xColumn);
    }

    public Dictionary<double, double> CreateAccumulatorForStacking()
    {
        var fullXdata = _result.EnumerateColumnData(_xColumn).ToArray();
        return fullXdata.Distinct().ToDictionary(k => _xLookup.ValueFor(k), _ => 0.0);
    }

    public void AssignSeriesNameColumn(int i)
    {
        _seriesNameColumn = _result.ColumnDefinitions()[i];
    }

    public void AssignValueColumn(int i)
    {
        _valueColumn = _result.ColumnDefinitions()[i];
        _valueLookup= CreateLookup(_valueColumn);
    }

    public IReadOnlyCollection<ChartSeries> CalculateSeries()
    {
        var series = _result.EnumerateRows()
            .GroupBy(r => r[_seriesNameColumn.Index])
            .ToArray();

        var all = new List<ChartSeries>();
        
        foreach (var (index,s) in series.Index())
        {
            var x = s.Select(r => _xLookup.ValueFor(r[_xColumn.Index])).ToArray();
            var y = s.Select(r => _valueLookup!.ValueFor(r[_valueColumn.Index])).ToArray();
           var legend = s.Key!.ToString().NullToEmpty();
           all.Add(new ChartSeries(index,legend,x,y));
        }

        return all;
    }
    public readonly record struct ChartSeries(int Index,string Legend, double[] X, double[] Y);

    public bool XisDateTime => _xColumn.UnderlyingType == typeof(DateTime);
    public bool IsNominal(ColumnResult column)=> column.UnderlyingType == typeof(string);
    public bool XisNominal => IsNominal(_xColumn);
    public bool YisNominal => IsNominal(_valueColumn);
    public bool YisDateTime =>_valueColumn.UnderlyingType == typeof(DateTime);

    public Tick[] GetTicks(IAxisLookup lookup)
    {
        return
            lookup.Dict().Select(kv =>
                new Tick(kv.Key, kv.Value)).ToArray();
    }
    public Tick[] GetXTicks() => GetTicks(_xLookup);
    public Tick[] GetYTicks() => GetTicks(_valueLookup);

    public double GetSuggestedBarWidth()
    {
        if (XisDateTime)
        {
            var smallestGap = _result
                .EnumerateColumnData(_xColumn)
               .OfType<DateTime>()
                .Distinct()
                .OrderBy(d => d)
                .Pairwise((a, b) => (b - a).TotalDays)
                .Min();
            return smallestGap*0.9;
        }
        else return 0.0;
    }
}
