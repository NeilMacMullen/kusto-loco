using System.Diagnostics;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using Extensions;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using NLog;

namespace KustoSupport;

/// <summary>
///     Provides a context for complex queries or persistent tables
/// </summary>
public class KustoQueryContext
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly BabyKustoEngine _engine = new();
    private readonly List<ITableSource> _tables = new();

    private bool _fullDebug;

    private IKustoQueryContextTableLoader _lazyTableLoader = new NullTableLoader();


    public IEnumerable<string> TableNames => Tables().Select(t => t.Name);

    public void AddTable(TableBuilder builder)
        => AddTable(builder.ToTableSource());


    public void AddTable(ITableSource table)
    {
        if (_tables.Any(t => t.Name == table.Name))
            throw new ArgumentException($"Context already contains a table named '{table.Name}'");
        _tables.Add(table);
        _engine.AddGlobalTable(table);
    }

    public ITableSource GetTable(string name)
    {
        return _tables.Single(t => UnescapeTableName(t.Name) == name);
    }

    public void AddTableFromRecords<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var table = TableBuilder.CreateFromRows(tableName, records);
        AddTable(table);
    }


    public void AddChunkedTableFromRecords<T>(string tableName, IReadOnlyCollection<T> records, int chunkSize)
    {
        var table = TableBuilder.CreateFromRows(tableName, records);
        var chunked = ChunkedKustoTable
            .FromTable(table.ToTableSource(), chunkSize);
        AddTable(chunked);
    }

    public int BenchmarkQuery(string query)
    {
        var res = _engine.Evaluate(query, _fullDebug, _fullDebug);
        return res.RowCount;
    }

    //temporary until we plumb lazy table load back in
    public Task<KustoQueryResult> RunTabularQueryAsync(string query) => Task.FromResult(RunTabularQuery(query));

    public KustoQueryResult RunTabularQuery(string query)
    {
        var watch = Stopwatch.StartNew();
        //handling for "special" commands
        if (query.Trim() == ".tables")
        {
            //TODO
        }

        try
        {
            var result =
                _engine.Evaluate(query,
                    _fullDebug, _fullDebug
                );
            var (table, vis) = TableFromEvaluationResult(result);

            return new KustoQueryResult(query, table, vis, (int)watch.ElapsedMilliseconds,
                string.Empty);
        }
        catch (Exception ex)
        {
            var (table, vis) = TableFromEvaluationResult(EvaluationResult.Null);
            return new KustoQueryResult(query, table, vis, 0, ex.Message);
        }
    }

    private (InMemoryTableSource, VisualizationState) TableFromEvaluationResult(EvaluationResult results)
    {
        switch (results)
        {
            case ScalarResult scalar:

                return (InMemoryTableSource.FromITableSource(TableBuilder.FromScalarResult(scalar))
                    , VisualizationState.Empty);
            case TabularResult tabular:

                return (InMemoryTableSource.FromITableSource(tabular.Value), tabular.VisualizationState);

            default:

                return (new InMemoryTableSource(TableSymbol.Empty, Array.Empty<BaseColumn>()),
                    VisualizationState.Empty);
        }
    }

    public void AddLazyTableLoader(IKustoQueryContextTableLoader loader) => _lazyTableLoader = loader;

    private async Task<KustoQueryResult> RunQuery(string query)
    {
        try
        {
            // Get tables referenced in query
            var requiredTables = GetTableList(query);

            // Ensure that tables are loaded into the query context
            await _lazyTableLoader.LoadTablesAsync(this, requiredTables);

            // Note: Hold the lock until the query is complete to ensure that tables don't change
            // in the middle of execution.
            return await RunTabularQueryAsync(query);
        }
        catch (Exception ex)
        {
            var (table, vis) = TableFromEvaluationResult(EvaluationResult.Null);
            return new KustoQueryResult(query, table, vis, 0, ex.Message);
        }
    }


    /// <summary>
    ///     Gets a list of all table references within a Kusto query
    /// </summary>
    /// <remarks>
    ///     Taken from
    ///     https://stackoverflow.com/questions/73322172/putting-all-table-names-that-a-kql-query-uses-into-a-list-in-c-sharp
    /// </remarks>
    public static IReadOnlyCollection<string> GetTableList(string query)
    {
        var tables = new List<string>();
        var code = KustoCode.Parse(query).Analyze();

        SyntaxElement.WalkNodes(code.Syntax,
            @operator =>
            {
                if (@operator is Expression e && e.RawResultType is TableSymbol &&
                    @operator.Kind.ToString() == "NameReference")
                    tables.Add(e.RawResultType.Name);
            });
        // //special case handling for when query is _only_ a table name without any operators
        if (!tables.Any() && query.Tokenise().Length == 1)
        {
            tables.Add(query.Trim());
        }

        return tables.Select(UnescapeTableName).Distinct().ToArray();
    }

    public IEnumerable<ITableSource> Tables() => _tables;

    public static string UnescapeTableName(string tableName)
    {
        if ((tableName.StartsWith("['") && tableName.EndsWith("']")) ||
            (tableName.StartsWith("[\"") && tableName.EndsWith("\"]"))
           )
            return tableName.Substring(2, tableName.Length - 4);
        return tableName;
    }

    public static string EnsureEscapedTableName(string tableName) => $"['{UnescapeTableName(tableName)}']";

    public static KustoQueryContext WithFullDebug()
    {
        var context = new KustoQueryContext
        {
            _fullDebug = true
        };
        return context;
    }
}