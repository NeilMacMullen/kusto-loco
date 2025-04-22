using System.Numerics;
using KustoLoco.Core;
using NotNullStrings;
using static MoreLinq.Extensions.PairwiseExtension;

namespace KustoLoco.Rendering.ScottPlot;

public class ResultChartAccessor
{
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

    private readonly KustoQueryResult _result;
    private ColumnResult _seriesNameColumn = ColumnResult.Empty;
    private ColumnResult _valueColumn = ColumnResult.Empty;
    private IAxisLookup _valueLookup = new NumericAxisLookup<double>();
    private ColumnResult _xColumn = ColumnResult.Empty;
    private IAxisLookup _xLookup = new NumericAxisLookup<double>();

    public ResultChartAccessor(KustoQueryResult result)
    {
        _result = result;
    }

    public bool XisDateTime => IsTemporal(_xColumn);
    public bool XisNominal => IsNominal(_xColumn);
    public bool YisNominal => IsNominal(_valueColumn);
    public bool YisDateTime => IsTemporal(_valueColumn);

    public bool IsTemporal(ColumnResult column) => column.UnderlyingType == typeof(DateTime);

    public bool IsNumeric(ColumnResult column) =>
        new[] { typeof(double), typeof(long), typeof(float), typeof(int) }.Contains(column.UnderlyingType);

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
        return fullXdata.Distinct().ToDictionary(k => _xLookup.AxisValueFor(k), _ => 0.0);
    }

    public void AssignSeriesNameColumn(int i)
    {
        if (i < _result.ColumnCount)
            _seriesNameColumn = _result.ColumnDefinitions()[i];
    }

    public void AssignValueColumn(int i)
    {
        _valueColumn = _result.ColumnDefinitions()[i];
        _valueLookup = CreateLookup(_valueColumn);
    }

    private object? SeriesValue(object?[] row)
        => _seriesNameColumn == ColumnResult.Empty
            ? string.Empty
            : row[_seriesNameColumn.Index];

    public IReadOnlyCollection<ChartSeries> CalculateSeries()
    {
        var series = _result.EnumerateRows()
            .GroupBy(SeriesValue)
            .ToArray();

        var all = new List<ChartSeries>();

        foreach (var (index, s) in series.Index())
        {
            var x = s.Select(r => _xLookup.AxisValueFor(r[_xColumn.Index])).ToArray();
            var y = s.Select(r => _valueLookup.AxisValueFor(r[_valueColumn.Index])).ToArray();
            var legend = s.Key!.ToString().NullToEmpty();
            all.Add(new ChartSeries(index, legend, x, y));
        }

        return all;
    }

    /// <summary>
    ///     True if the type of column represents an unordered, non-numeric value such as a name
    /// </summary>
    public bool IsNominal(ColumnResult column) =>
        new[] { typeof(string), typeof(bool), typeof(Guid) }.Contains(column.UnderlyingType);

    public GenericTick[] GetTicks(IAxisLookup lookup) =>
        lookup.AxisValuesAndLabels().Select(kv =>
            new GenericTick(kv.Key, kv.Value)).ToArray();

    public GenericTick[] GetXTicks() => GetTicks(_xLookup);
    public GenericTick[] GetYTicks() => GetTicks(_valueLookup);

    public double GetSuggestedBarWidth()
    {
        if (_result.RowCount < 1)
            return 1.0;
        if (XisDateTime)
        {
          
            var smallestGap = _result
                .EnumerateColumnData(_xColumn)
                .OfType<DateTime>()
                .Distinct()
                .OrderBy(d => d)
                .Pairwise((a, b) => (b - a).TotalDays)
                .Min();
            return smallestGap * 0.9;
        }

        if (IsNumeric(_xColumn))
        {
            var smallestGap = _result
                .EnumerateColumnData(_xColumn)
                .Select(d=>Convert.ChangeType(d,typeof(double)))
                .OfType<double>()
                .Distinct()
                .OrderBy(d => d)
                .Pairwise((a, b) => b-a)
                .Min();
            return smallestGap * 0.9;
        }
        return 1.0;
    }

    public string GetXLabel() => _xColumn.Name;

    public string GetYLabel() => _valueColumn.Name;

    /// <summary>
    ///     Try to reorder the columns in a way that makes most sense to the
    ///     particular chart
    /// </summary>
    /// <remarks>
    ///     We use a little DSL here to specify desired orderings....
    ///     "tno|too" means we prefer Time, Numeric, nOminal or, failing that
    ///     Time, nOminal,nOminal
    /// </remarks>
    public ColumnResult[] TryOrdering(string cn)
    {
        var combos = cn.Tokenize("|");
        var availableColumns = _result.ColumnDefinitions();
        foreach (var axisCombo in combos)
        {
            if (availableColumns.Length < axisCombo.Length)
                continue;

            var claimed = new List<ColumnResult>();
            var chars = axisCombo.ToLowerInvariant().ToCharArray();

            AllowOverrideFrom("xcolumn", 0);
            AllowOverrideFrom("ycolumns", 1);
            AllowOverrideFrom("series", 2);
            foreach (var ch in chars)
                switch (ch)
                {
                    case '_':
                        //indicates we've already handled this
                        break;
                    case 'n':
                        ClaimFirstMatchingColumn(IsNumeric);
                        break;
                    case 't':
                        ClaimFirstMatchingColumn(IsTemporal);
                        break;
                    case 'o':
                        ClaimFirstMatchingColumn(IsNominal);
                        break;
                }

            if (claimed.Count == axisCombo.Length)
                return claimed.ToArray();
            continue;

            void AllowOverrideFrom(string property, int index)
            {
                ClaimFirstMatchingColumn(
                    col => col.Name == _result.Visualization.PropertyOr(property, string.Empty));
                if (claimed.Any())
                    chars[index] = '_';
            }

            void ClaimFirstMatchingColumn(Func<ColumnResult, bool> criteria)
            {
                var matches = availableColumns.Except(claimed)
                    .OrderBy(c => c.Index)
                    .Where(criteria).ToArray();
                if (matches.Any())
                    claimed.Add(matches.First());
            }
        }

        //if all else fails return the original set...
        return availableColumns;
    }
    /// <summary>
    /// Represents a plottable chart series for ScottPlot
    /// </summary>
    public readonly record struct ChartSeries(int Index, string Legend, double[] X, double[] Y)
    {
        /// <summary>
        /// Return a version of the ChartSeries with the X and Y values ordered by X
        /// </summary>
        public ChartSeries OrderByX()
        {
            var pairs = X.Zip(Y)
                .OrderBy(tuple => tuple.First)
                .ThenBy(tuple => tuple.Second)
                .ToArray();
            return new ChartSeries(Index, Legend,
                    pairs.Select(t => t.First).ToArray(),
                    pairs.Select(t => t.Second).ToArray())
                ;
        }
    }
}
