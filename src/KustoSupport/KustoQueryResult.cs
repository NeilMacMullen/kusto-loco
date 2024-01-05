using System.Collections.Specialized;
using System.Text.Json;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Util;
using NLog;

#pragma warning disable CS8603 // Possible null reference return.

namespace KustoSupport;

public class KustoQueryResult
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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


    private InMemoryTableSource Table { get; }
    public int QueryDuration { get; init; }
    public string Error { get; init; }
    public VisualizationState Visualization { get; init; }
    public int Height { get; init; }

    public int Width => ColumnDefinitions().Length;

    public object?[] GetRow(int row)
        => Enumerable.Range(0, Width)
            .Select(c => Get(c, row)).ToArray();

    public IEnumerable<object?[]> EnumerateRows() =>
        Enumerable.Range(0, Height)
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

    public IReadOnlyCollection<OrderedDictionary> AsOrderedDictionarySet()
    {
        var items = new List<OrderedDictionary>();

        var columns = ColumnDefinitions();
        var chunk = Table.GetData().First();
        for (var row = 0; row < Height; row++)
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
    public IReadOnlyCollection<T> DeserialiseTo<T>()
    {
        //this is horrible but I don't have time to research how to do it ourselves and the bottom line
        //is that we are expecting results sets to be small to running through the JsonSerializer is
        //"good enough" for now...

        var json = ToJsonString();
        return JsonSerializer.Deserialize<T[]>(json);
    }

    /// <summary>
    ///     Deserialises a Dictionary-based result to objects
    /// </summary>
    public string ToJsonString() => JsonSerializer.Serialize(AsOrderedDictionarySet());
}