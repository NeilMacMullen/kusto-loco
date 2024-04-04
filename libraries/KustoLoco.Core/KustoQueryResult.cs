using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text.Json;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Util;
using NLog;
using NotNullStrings;

#pragma warning disable CS8603 // Possible null reference return.

namespace KustoLoco.Core;

public class KustoQueryResult
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static readonly KustoQueryResult Empty = new(string.Empty,
        InMemoryTableSource.Empty, VisualizationState.Empty, 0, string.Empty);

    /// <summary>
    ///     Provides the results of a Kusto query as a collection of dictionaries
    /// </summary>
    /// <remarks>
    ///     The client really needs to have some idea of the expected schema to use this layer of API
    ///     In the future we may provide a more "column-oriented" result where the type of the columns
    ///     is explicitly provided but for the moment this allows easy json serialisation
    /// </remarks>
    public KustoQueryResult(string query,
        InMemoryTableSource results,
        VisualizationState vis,
        int queryDuration,
        string error)
    {
        Query = query;
        Table = results;
        Visualization = vis;
        Height = results.RowCount;

        QueryDuration = queryDuration;
        Error = error;
    }

    public string Query { get; init; }


    public InMemoryTableSource Table { get; }
    public int QueryDuration { get; init; }
    public string Error { get; init; }
    public VisualizationState Visualization { get; init; }
    public int Height { get; init; }

    public int Width => ColumnDefinitions().Length;

    public object?[] GetRow(int row)
        => Enumerable.Range(0, Width)
            .Select(c => Get(c, row))
            .ToArray();

    public IEnumerable<object?[]> EnumerateRows()
        => Enumerable.Range(0, Height)
            .Select(GetRow);

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

    public string[] ColumnNames() => ColumnDefinitions().Select(c => c.Name).ToArray();

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
        var rowsToTake = Math.Min(max, Height);
        for (var row = 0; row < rowsToTake; row++)
        {
            var d = new OrderedDictionary();

            for (var col = 0; col < columns.Length; col++)
            {
                var dataValue = chunk.Columns[col].GetRawDataValue(row);
                var columnName = columns[col].Name;
                d[columnName] = dataValue;
            }

            items.Add(d);
        }

        return items;
    }

    public IEnumerable<object?> EnumerateColumnData(ColumnResult col) => Table.GetColumnData(col.Index);

    /// <summary>
    ///     Deserialises a Dictionary-based result to objects
    /// </summary>
    public IReadOnlyCollection<T> ToRecords<T>(int max = int.MaxValue)
        => ToRecords<T>(AsOrderedDictionarySet(max));

    public static IReadOnlyCollection<T> ToRecords<T>(IEnumerable<OrderedDictionary> dictionaries)
    {
        //this is horrible but I don't have time to research how to do it ourselves and the bottom line
        //is that we are expecting results sets to be small to running through the JsonSerializer is
        //"good enough" for now...

        var json = ToJsonString(dictionaries);
        return JsonSerializer.Deserialize<T[]>(json);
    }

    public string ToJsonString() => ToJsonString(AsOrderedDictionarySet());
    public static string ToJsonString(object o) => JsonSerializer.Serialize(o);

    public IReadOnlyCollection<OrderedDictionary> ResultOrErrorAsSet()
        => Error.IsNotBlank()
            ? [new OrderedDictionary { ["QUERY ERROR"] = Error }]
            : AsOrderedDictionarySet();


    /// <summary>
    ///     Allows a KustoQueryResult to dropped into a DTO or other serializable object
    /// </summary>
    /// <remarks>
    ///     We currently use an OrderedDictionary to represent the rows of the result set.
    ///     However in future we may return a JsonObject
    /// </remarks>
    public object ToSerializableObject() => AsOrderedDictionarySet();

    /// <summary>
    /// Returns the result of a query as a DataTable
    /// </summary>
    public DataTable ToDataTable(int maxRows=Int32.MaxValue)
    {
        var dt = new DataTable();

        foreach (var col in ColumnNames())
            dt.Columns.Add(col);

        foreach (var row in EnumerateRows().Take(maxRows))
            dt.Rows.Add(row);
        return dt;

    }
}