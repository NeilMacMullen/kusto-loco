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
        new[] { typeof(decimal),typeof(double), typeof(long), typeof(float), typeof(int) }.Contains(column.UnderlyingType);

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
        var columns = _result.ColumnDefinitions();
        // Check if all columns are numeric
        if (columns.Length > 1
            && IsNumeric(columns[0]) || IsTemporal(columns[0])
            && columns.Skip(1).All(IsNumeric))
        {
            var xCol = columns[0];
            var yCols = columns.Skip(1).ToArray();
            var rowCount = _result.RowCount;
            var xData = _result.EnumerateColumnData(xCol)
                .Select(Convert.ToDouble)
                .ToArray();

            var all = new List<ChartSeries>();
            for (var i = 0; i < yCols.Length; i++)
            {
                var yCol = yCols[i];
                var yData = _result.EnumerateColumnData(yCol)
                    .Select(Convert.ToDouble)
                    .ToArray();

                // Use column name as legend, index as color
                var legend = yCol.Name.OrWhenBlank("_none");
                var color = Enumerable.Repeat(i, rowCount).ToArray();

                all.Add(new ChartSeries(i, legend, xData, yData, color));
            }
            return all;
        }

        // Default grouping by series value
        var series = _result.EnumerateRows()
            .GroupBy(SeriesValue)
            .ToArray();

        var allDefault = new List<ChartSeries>();

        //allow point colors to be specifically defined
        var colorColumn = _result.ColumnDefinitions()
            .Where(c => c.Name == "_color" && c.UnderlyingType == typeof(long))
            .ToArray();

        var colorIndex = colorColumn.Any() ? colorColumn.First().Index : -1;

        foreach (var (index, s) in series.Index())
        {
            var x = s.Select(r => _xLookup.AxisValueFor(r[_xColumn.Index])).ToArray();
            var y = s.Select(r => _valueLookup.AxisValueFor(r[_valueColumn.Index])).ToArray();
            var legend = (s.Key?.ToString() ?? "_null").OrWhenBlank("_none");
            var c = colorIndex < 0
                ? s.Select(_ => index).ToArray()
                : s.Select(r => (int)((long?)r[colorIndex] ?? 0)).ToArray();

            allDefault.Add(new ChartSeries(index, legend, x, y, c));
        }

        return allDefault;
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
        if (_result.RowCount < 2)
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
                .Select(d => Convert.ChangeType(d, typeof(double)))
                .OfType<double>()
                .Distinct()
                .OrderBy(d => d)
                .Pairwise((a, b) => b - a)
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
                    case 'x':
                        ClaimFirstMatchingColumn(_=>true);
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
    ///     Represents a plottable chart series for ScottPlot
    /// </summary>
    public readonly record struct ChartSeries(double Index, string Legend, double[] X, double[] Y,int[] Color)
    {
        /// <summary>
        ///     Return a version of the ChartSeries with the X and Y values ordered by X
        /// </summary>
        public ChartSeries OrderByX()
        {
            var rows = X.Zip(Y,Color)
                .OrderBy(tuple => tuple.First)
                .ThenBy(tuple => tuple.Second)
                .ToArray();
            return new ChartSeries(Index, Legend,
                    rows.Select(t => t.First).ToArray(),
                    rows.Select(t => t.Second).ToArray(),
                    rows.Select(t=>t.Third).ToArray()
                    )
                ;
        }

        public ChartSeries AccumulateY()
        {
            var accumulatedY = new double[Y.Length];
            accumulatedY[0] = Y[0];

            for (var i = 1; i < Y.Length; i++)
                accumulatedY[i] = accumulatedY[i - 1] + Y[i];

            return this with { Y = accumulatedY };
        }
    }
}
