using System.Collections.Immutable;
using System.Diagnostics;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Evaluation.BuiltIns;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using NLog;
using NotNullStrings;

namespace KustoSupport;

/// <summary>
///     Provides a context for complex queries or persistent tables
/// </summary>
public class KustoQueryContext
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private Dictionary<FunctionSymbol, ScalarFunctionInfo> _additionalFunctions = new();

    private bool _fullDebug;

    private IKustoQueryContextTableLoader _lazyTableLoader = new NullTableLoader();

    private List<ITableSource> _tables = [];

    public IEnumerable<string> TableNames => Tables().Select(t => t.Name);

    //TODO - ugh - don't like exposing this in this way
    public void AddFunctions(Dictionary<FunctionSymbol, ScalarFunctionInfo> additionalFunctions)
    {
        _additionalFunctions = additionalFunctions;
    }


    public void AddTable(TableBuilder builder) => AddTable(builder.ToTableSource());


    public void AddTable(ITableSource table)
    {
        if (_tables.Any(t => t.Name == table.Name))
            throw new ArgumentException($"Context already contains a table named '{table.Name}'");
        RemoveTable(table.Name);
        _tables.Add(table);
    }


    /// <summary>
    ///     Remove the named table if it exists, otherwise do nothing
    /// </summary>
    /// <remarks>
    ///     Supplied name may be framed with escapes
    /// </remarks>
    public void RemoveTable(string tableName)
    {
        tableName = KustoNameEscaping.RemoveFraming(tableName);
        _tables = _tables.Where(t => t.Name != tableName).ToList();
    }


    /// <summary>
    ///     Renames a table
    /// </summary>
    /// <remarks>
    ///     Will overwrite any existing table with the new name
    /// </remarks>
    public void RenameTable(string oldName, string newName)
    {
        ShareTable(oldName, newName);
        RemoveTable(oldName);
    }

    public void OldAddTableFromRecords<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var table = TableBuilder.OldCreateFromRows(tableName, records);
        AddTable(table);
    }

    public void AddTableFromRecords<T>(string tableName, ImmutableArray<T> records)
    {
        var table = TableBuilder.CreateFromRows(tableName, records);
        AddTable(table);
    }


    public void AddChunkedTableFromRecords<T>(string tableName, IReadOnlyCollection<T> records, int chunkSize)
    {
        var table = TableBuilder.OldCreateFromRows(tableName, records);
        var chunked = ChunkedKustoTable
            .FromTable(table.ToTableSource(), chunkSize);
        AddTable(chunked);
    }

    public int BenchmarkQuery(string query)
    {
        var engine = new BabyKustoEngine();
        var res = engine.Evaluate(_tables, query, _fullDebug, _fullDebug);
        return res.RowCount;
    }

    public KustoQueryResult RunTabularQueryWithoutDemandBasedTableLoading(string query)
    {
        var watch = Stopwatch.StartNew();
        var engine = new BabyKustoEngine();
        engine.AddAdditionalFunctions(_additionalFunctions);
        //handling for "special" commands
        if (query.Trim() == ".tables")
            return CreateTableList(query, false);

        try
        {
            var result =
                engine.Evaluate(_tables, query,
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

    private KustoQueryResult CreateTableList(string query, bool expandColumns)
    {
        if (expandColumns)
        {
            var rows = _tables.SelectMany(table
                    => table.Type.Columns.Select((c, i) =>
                        new
                        {
                            Table = table.Name,
                            Column = c.Name,
                            Type = c.Type.Name,
                            Ordinal = i
                        }
                    ))
                .ToImmutableArray();
            ;

            var tr = TableBuilder.CreateFromRows("tables", rows)
                .ToTableSource() as InMemoryTableSource;

            return new KustoQueryResult(query, tr!, VisualizationState.Empty, 0, string.Empty);
        }
        else
        {
            var rows = _tables.Select(table => new { Table = table.Name, Columns = table.Type.Columns.Count })
                    .ToImmutableArray()
                ;

            var tr = TableBuilder.CreateFromRows("tables", rows)
                .ToTableSource() as InMemoryTableSource;

            return new KustoQueryResult(query, tr!, VisualizationState.Empty, 0, string.Empty);
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

    public async Task<KustoQueryResult> RunTabularQueryAsync(string query)
    {
        try
        {
            // Get tables referenced in query
            var requiredTables = GetTableList(query);

            // Ensure that tables are loaded into the query context
            await _lazyTableLoader.LoadTablesAsync(this, requiredTables);

            return RunTabularQueryWithoutDemandBasedTableLoading(query);
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
            op =>
            {
                if (op is Expression { RawResultType: TableSymbol } e &&
                    op.Kind.ToString() == "NameReference")
                    tables.Add(e.RawResultType.Name);
            });
        //special case handling for when query is _only_ a table name without any operators
        if (!tables.Any() && query.Tokenize().Length == 1)
        {
            tables.Add(query.Trim());
        }

        return tables.Select(KustoNameEscaping.RemoveFraming).Distinct().ToArray();
    }

    public IEnumerable<ITableSource> Tables() => _tables;


    public static KustoQueryContext WithFullDebug()
    {
        var context = new KustoQueryContext
        {
            _fullDebug = true
        };
        return context;
    }

    /// <summary>
    ///     Makes an existing table available under a different name
    /// </summary>
    /// <remarks>
    ///     If the target name is already in use, the old table will be removed
    /// </remarks>
    public void ShareTable(string sourceName, string requestedTableName)
    {
        sourceName = KustoNameEscaping.RemoveFraming(sourceName);
        requestedTableName = KustoNameEscaping.RemoveFraming(requestedTableName);
        var matches = _tables.Where(t => t.Name == sourceName).ToArray();
        if (matches.Any())
        {
            //in case there's already a table under the target name, remove it...
            RemoveTable(requestedTableName);
            var sharedTable = TableBuilder.FromTable(matches.First(), requestedTableName);
            AddTable(sharedTable);
        }
    }

    public async Task<IReadOnlyCollection<T>> RunTabularQueryToRecordSet<T>(string query)
    {
        var result = await RunTabularQueryAsync(query);
        if (result.Error.IsNotBlank())
            throw new InvalidOperationException($"{result.Error}");
        return result.DeserialiseTo<T>();
    }

    /// <summary>
    ///     Takes the results from an earlier query and materializes them as a table
    /// </summary>
    public void MaterializeResultAsTable(KustoQueryResult queryResult, string tableName)
    {
        var mat = TableBuilder.FromTable(queryResult.Table,
            KustoNameEscaping.RemoveFraming(tableName)
        );
        AddTable(mat);
    }
}