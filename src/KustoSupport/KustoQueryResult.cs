using System.Collections.Specialized;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

namespace KustoSupport;

#pragma warning disable CS8601
public class KustoQueryResult
{
    /// <summary>
    ///     Provides the results of a Kusto query as a collection of dictionaries
    /// </summary>
    /// <remarks>
    ///     The client really needs to have some idea of the expected schema to use this layer of API
    ///     In the future we may provide a more "column-oriented" result where the type of the columns
    ///     is explicitly provided but for the moment this allows easy json serialisation
    /// </remarks>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public KustoQueryResult(string Query,
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        EvaluationResult results,
        int QueryDuration,
        string Error)
    {
        this.Query = Query;
        switch (results)
        {
            case ScalarResult scalar:
                Visualization = VisualizationState.Empty;
                var s = TableBuilder.FromScalarResult(scalar);
                Table = InMemoryTableSource.FromITableSource(s);
                Height = 1;
                break;
            case TabularResult tabular:
                Visualization = tabular.VisualizationState;
                var sourceTable = tabular.Value;
                Table = InMemoryTableSource.FromITableSource(sourceTable);
                Height = Table.GetData().Single().RowCount;
                break;
            default:
                Visualization = VisualizationState.Empty;
                Table = new InMemoryTableSource(TableSymbol.Empty, Array.Empty<BaseColumn>());
                Height = 0;
                break;
        }


        this.QueryDuration = QueryDuration;
        this.Error = Error;
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
        return Table.Type.Columns.Select(c => new ColumnResult(c.Name,
            TypeMapping.TypeForSymbol(c.Type))).ToArray();
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

        var table = Table;
        var columns = ColumnDefinitions();
        var chunk = table.GetData().First();
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
}