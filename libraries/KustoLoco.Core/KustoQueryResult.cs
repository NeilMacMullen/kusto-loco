using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text.Json;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Util;

using NotNullStrings;

namespace KustoLoco.Core;

public class KustoQueryResult
{
    

    public static readonly KustoQueryResult Empty = new(string.Empty,
        InMemoryTableSource.Empty, VisualizationState.Empty, TimeSpan.Zero, string.Empty);

    /// <summary>
    ///     Provides the results of a Kusto query as a collection of dictionaries
    /// </summary>
    /// <remarks>
    ///     The client really needs to have some idea of the expected schema to use this layer of API
    ///     In the future we may provide a more "column-oriented" result where the type of the columns
    ///     is explicitly provided but for the moment this allows easy json serialisation
    /// </remarks>
    public KustoQueryResult(string query,
        IMaterializedTableSource results,
        VisualizationState vis,
        TimeSpan queryDuration,
        string error)
    {
        Query = query;
        Table = results;
        Visualization = vis;
        RowCount = results.RowCount;

        QueryDuration = queryDuration;
        Error = error;
    }

    /// <summary>
    ///     Original query that generated the result
    /// </summary>
    public string Query { get; init; }


    public IMaterializedTableSource Table { get; }

    /// <summary>
    ///     Duration of query execution
    /// </summary>
    public TimeSpan QueryDuration { get; init; }

    /// <summary>
    ///     Error message if the query failed, otherwise string.Empty
    /// </summary>
    public string Error { get; init; }

    /// <summary>
    ///     Visualization state of the result (i.e. whether it's a table, chart, etc)
    /// </summary>
    public VisualizationState Visualization { get; init; }

    /// <summary>
    ///     Number of rows in the result
    /// </summary>
    public int RowCount { get; init; }

    /// <summary>
    ///     Number of columns in the result
    /// </summary>
    public int ColumnCount => ColumnDefinitions().Length;

    /// <summary>
    /// True if the result should be visualised as a chart 
    /// </summary>
    public bool IsChart =>
        Visualization.ChartType.IsNotBlank() && Visualization.ChartType.ToLowerInvariant() != "table";

    public static KustoQueryResult FromError(string query, string error)
    {
        return new KustoQueryResult(query, InMemoryTableSource.Empty, VisualizationState.Empty, TimeSpan.Zero, error);
    }

    /// <summary>
    ///     Gets an array of nullable objects representing cells in the row at the given index
    /// </summary>
    /// <remarks>
    ///     Kusto uses null to represent missing values so we need to use nullable types here
    /// </remarks>
    public object?[] GetRow(int row)
    {
        return Enumerable.Range(0, ColumnCount)
            .Select(c => Get(c, row))
            .ToArray();
    }

    /// <summary>
    ///     Enumerate over all rows in the result
    /// </summary>
    public IEnumerable<object?[]> EnumerateRows(int maxCount)
    =>  Enumerable.Range(0, Math.Min(RowCount,maxCount))
            .Select(GetRow);

    public IEnumerable<object?[]> EnumerateRows()
        => EnumerateRows(RowCount);

    /// <summary>
    ///     Returns the column definitions for the result
    /// </summary>
    /// <remarks>
    ///     A ColumnResult is a simple struct that contains the name index and underlying C# type of a column
    /// </remarks>
    public ColumnResult[] ColumnDefinitions()
    {
        return Table.Type
            .Columns
            .Select((c, i) => new ColumnResult(
                c.Name,
                i,
                TypeMapping.UnderlyingTypeForSymbol(c.Type))
            )
            .ToArray();
    }

    /// <summary>
    ///     Returns the names of the columns in the result
    /// </summary>
    public string[] ColumnNames()
    {
        return ColumnDefinitions().Select(c => c.Name).ToArray();
    }

    /// <summary>
    ///     Fetch a particular cell in the result
    /// </summary>
    public object? Get(int col, int row)
    {
        var chunk = Table.GetData().First();
        return chunk.Columns[col].GetRawDataValue(row);
    }

    public IReadOnlyCollection<OrderedDictionary> AsOrderedDictionarySet(int max = int.MaxValue)
    {
        var items = new List<OrderedDictionary>();

        var columns = ColumnDefinitions();
        var chunk = Table.GetData().First();
        var rowsToTake = Math.Min(max, RowCount);
        for (var row = 0; row < rowsToTake; row++)
        {
            var d = RowToDictionary(row);
            items.Add(d);
        }

        return items;
    }

    public OrderedDictionary RowToDictionary(int row)
    {
        var d = new OrderedDictionary();
        var columns = ColumnDefinitions();
        for (var col = 0; col < columns.Length; col++)
        {
            var dataValue =Get(col,row);
            var columnName = columns[col].Name;
            d[columnName] = dataValue;
        }
        return d;

    }
    /// <summary>
    ///     For a particular column enumerate over all cells
    /// </summary>
    public IEnumerable<object?> EnumerateColumnData(ColumnResult col)
    {
        return Table.GetColumnData(col.Index);
    }

    /// <summary>
    ///     Deserialises a Dictionary-based result to objects
    /// </summary>
    public IReadOnlyCollection<T> ToRecords<T>(int max = int.MaxValue)
    {
        return ToRecords<T>(AsOrderedDictionarySet(max));
    }

    public static IReadOnlyCollection<T> ToRecords<T>(IEnumerable<OrderedDictionary> dictionaries)
    {
        //this is horrible but I don't have time to research how to do it ourselves and the bottom line
        //is that we are expecting results sets to be small to running through the JsonSerializer is
        //"good enough" for now...

        var json = ToJsonString(dictionaries);
        try
        {
            return JsonSerializer.Deserialize<T[]>(json)!;
        }
        catch
        {
            return [];
        }
    }

    public string ToJsonString()
    {
        return ToJsonString(AsOrderedDictionarySet());
    }

    public static string ToJsonString(object o)
    {
        return JsonSerializer.Serialize(o);
    }

    public IReadOnlyCollection<OrderedDictionary> ResultOrErrorAsSet()
    {
        return Error.IsNotBlank()
            ? [new OrderedDictionary { ["QUERY ERROR"] = Error }]
            : AsOrderedDictionarySet();
    }


    /// <summary>
    ///     Allows a KustoQueryResult to dropped into a DTO or other serializable object
    /// </summary>
    /// <remarks>
    ///     We currently use an OrderedDictionary to represent the rows of the result set.
    ///     However, in future we may return a JsonObject
    /// </remarks>
    public object ToSerializableObject()
    {
        return AsOrderedDictionarySet();
    }

    /// <summary>
    ///     Returns the result of a query as a DataTable, suitable for use in a DataGridView or similar
    /// </summary>
    public DataTable ToDataTable(int maxRows = int.MaxValue)
    {
        var dt = new DataTable();

        foreach (var col in ColumnDefinitions())
            dt.Columns.Add(col.Name, col.UnderlyingType);

        foreach (var row in EnumerateRows().Take(maxRows))
            dt.Rows.Add(row);

        return dt;
    }

    /// <summary>
    ///     Returns the result of a query as a DataTable, suitable for use in a DataGridView or similar
    /// </summary>
    /// <remarks>
    ///     If the result has an error, returns a single cell DataTable that contains the error text
    /// </remarks>
    public DataTable ToDataTableOrError(int maxRows = int.MaxValue)
    {
        if (Error.IsNotBlank())
        {
            var dt = new DataTable();
            dt.Columns.Add("ERROR");
            dt.Rows.Add(Error);
            return dt;
        }

        return ToDataTable(maxRows);
    }

    /// <summary>
    ///     Returns a page of the result
    /// </summary>
    public KustoQueryResult GetPage(int offset, int count)
    {
        if (RowCount == 0)
            return this;
        var pageOffset = Math.Min(RowCount, offset);
        var pageCount = Math.Min(RowCount - pageOffset, count);
        var pagedTable = PageOfKustoTable.Create(Table, pageOffset, pageCount);
        return new KustoQueryResult(Query, InMemoryTableSource.FromITableSource(pagedTable), Visualization,
            QueryDuration,
            Error);
    }

    /// <summary>
    /// Break a single result into multiple pages
    /// </summary>
    /// <remarks>
    /// When transferring large result sets over the network it can be useful to break them into pages
    /// So for example a client can issue a query that would return 100,000 rows and then paginate over them
    /// </remarks>
    public IReadOnlyCollection<KustoQueryResult> Paginate(int pageSize)
    {
        var pages = new List<KustoQueryResult>();

        for (var start = 0; start < RowCount; start += pageSize)
        {
            var page = GetPage(start, pageSize);
            pages.Add(page);
        }

        return pages;
    }
}
