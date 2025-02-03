using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.Settings;
using NLog;
using NotNullStrings;

namespace KustoLoco.Core;

/// <summary>
///     Provides a context for queries across a set of tables
/// </summary>
/// <remarks>
///     Querying is thread-safe, but adding/removing tables is not.  Care must be taken to
///     ensure that queries are not issued while tables are being added or removed. This needs
///     particular care when using lazy table loading since one query may cause the loaded table
///     list to change while another query is in progress.
///     A Fluent syntax is supported BUT the context is not immutable so operations will return the
///     original mutated context.
/// </remarks>
public class KustoQueryContext
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private Dictionary<FunctionSymbol, ScalarFunctionInfo> _additionalFunctions = [];
    private IKustoConsole _debugConsole = new SystemConsole();

    private IKustoQueryContextTableLoader _lazyTableLoader = new NullTableLoader();

    private List<ITableSource> _tables = [];
    private KustoSettingsProvider _settings=new KustoSettingsProvider();

    public IEnumerable<string> TableNames => Tables().Select(t => t.Name);

    //TODO - ugh - don't like exposing this in this way
    public void AddFunctions(Dictionary<FunctionSymbol, ScalarFunctionInfo> additionalFunctions)
    {
        _additionalFunctions = additionalFunctions;
    }

    /// <summary>
    ///     Adds a table to the context from a builder
    /// </summary>
    public KustoQueryContext AddTable(TableBuilder builder)
    {
        return AddTable(builder.ToTableSource());
    }

    /// <summary>
    ///     Adds a table to the context
    /// </summary>
    public KustoQueryContext AddTable(ITableSource table)
    {
        RemoveTable(table.Name);
        _tables.Add(table);
        return this;
    }


    /// <summary>
    ///     Remove the named table if it exists, otherwise do nothing
    /// </summary>
    /// <remarks>
    ///     Supplied name may be framed with escapes
    /// </remarks>
    public KustoQueryContext RemoveTable(string tableName)
    {
        tableName = KustoNameEscaping.RemoveFraming(tableName);
        _tables = _tables.Where(t => t.Name != tableName).ToList();
        return this;
    }


    /// <summary>
    ///     Renames a table
    /// </summary>
    /// <remarks>
    ///     Will overwrite any existing table with the new name
    /// </remarks>
    public KustoQueryContext RenameTable(string oldName, string newName)
    {
        ShareTable(oldName, newName);
        return RemoveTable(oldName);
    }

    /// <summary>
    ///     Adds a table to the context from volatile data
    /// </summary>
    /// <remarks>
    ///     This method is more convenient than the wrapping version but less efficient since it requires
    ///     a copy-and-convert operation. For smaller data sets, this is unlikely to be noticeable but
    ///     if operating on tables of 100Ks or millions of rows you should consider using the wrapped version.
    /// </remarks>
    public KustoQueryContext CopyDataIntoTable<T>(string tableName, IReadOnlyCollection<T> records)
    {
        return AddTable(TableBuilder.CreateFromVolatileData(tableName, records));
    }

    /// <summary>
    ///     Adds immutable data to the context
    /// </summary>
    /// <remarks>
    ///     This is the preferred way to add data to the context since it avoids additional allocations.
    ///     However, the client must ensure that the data is truly immutable and consists only of types
    ///     that are directly supported by Kusto.
    /// </remarks>
    public KustoQueryContext WrapDataIntoTable<T>(string tableName, ImmutableArray<T> records)
    {
        return AddTable(TableBuilder.CreateFromImmutableData(tableName, records));
    }

    /// <summary>
    ///     Runs a query and evaluates the result in order to get an accurate benchmark
    /// </summary>
    public int BenchmarkQuery(string query)
    {
        var engine = new BabyKustoEngine(new SystemConsole(),_settings);
        var res = engine.Evaluate(_tables, query);
        return res.RowCount;
    }

    /// <summary>
    ///     Runs a query against the context without attempting to load tables mentioned in the query
    /// </summary>
    /// <remarks>
    ///     This method skips the initial table presence check and is suitable for simple queries.  However, RunQuery
    ///     is preferred in most contexts since it has very little overhead and 'async' may become a requirement in future
    /// </remarks>
    public KustoQueryResult RunQueryWithoutDemandBasedTableLoading(string query)
    {
        var watch = Stopwatch.StartNew();
        var engine = new BabyKustoEngine(_debugConsole,_settings);
        engine.AddAdditionalFunctions(_additionalFunctions);
        //handling for "special" commands
        if (query.Trim() == ".tables")
            return CreateTableList(query, false);

        try
        {
            var result =
                engine.Evaluate(_tables, query);
            var (table, vis) = TableFromEvaluationResult(result);

            return new KustoQueryResult(query, table, vis, watch.Elapsed,
                string.Empty);
        }
        catch (Exception ex)
        {
            var (table, vis) = TableFromEvaluationResult(EvaluationResult.Null);
            return new KustoQueryResult(query, table, vis, TimeSpan.Zero, ex.Message);
        }
    }

    /// <summary>
    ///     Creates the table list for the ".tables" command
    /// </summary>
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

            var tr = TableBuilder.CreateFromImmutableData("tables", rows)
                .ToTableSource() as InMemoryTableSource;

            return new KustoQueryResult(query, tr!, VisualizationState.Empty, TimeSpan.Zero, string.Empty);
        }
        else
        {
            var rows = _tables.Select(table => new { Table = table.Name, Columns = table.Type.Columns.Count })
                .ToImmutableArray();

            var tr = TableBuilder.CreateFromImmutableData("tables", rows)
                .ToTableSource() as InMemoryTableSource;

            return new KustoQueryResult(query, tr!, VisualizationState.Empty, TimeSpan.Zero, string.Empty);
        }
    }

    /// <summary>
    ///     Converts an evaluation result into a table and visualization state so that it can be wrapped into a
    ///     KustoQueryResult
    /// </summary>
    /// <remarks>
    ///     We turn _scalar_ results into a single-value, single column table for consistency.
    /// </remarks>
    private static (InMemoryTableSource, VisualizationState) TableFromEvaluationResult(EvaluationResult results)
    {
        return results switch
        {
            ScalarResult scalar
                => (InMemoryTableSource.FromITableSource(TableBuilder.FromScalarResult("result", scalar)),
                    VisualizationState.Empty),
            TabularResult tabular
                => (InMemoryTableSource.FromITableSource(tabular.Value), tabular.VisualizationState),
            _
                => (new InMemoryTableSource(TableSymbol.Empty, []), VisualizationState.Empty)
        };
    }

    /// <summary>
    ///     Sets the table loader for the context
    /// </summary>
    /// <remarks>
    ///     This allows the context to lazily load tables as required by queries
    /// </remarks>
    public KustoQueryContext SetTableLoader(IKustoQueryContextTableLoader loader)
    {
        _lazyTableLoader = loader;
        return this;
    }

    /// <summary>
    ///     Runs a query against the context, loading tables as required
    /// </summary>
    /// <remarks>
    ///     If the context has a table loader, it will be used to load tables as required by the query
    ///     IMPORTANT - this call does not use ConfigureAwait(false) so use Task.Run() if you are calling
    ///     this from a UI thread
    /// </remarks>
    public async Task<KustoQueryResult> RunQuery(string query)
    {
        try
        {
            // Get tables referenced in query
            var requiredTables = GetTableList(query);

            // Ensure that tables are loaded into the query context
            await _lazyTableLoader.LoadTablesAsync(this, requiredTables);

            return RunQueryWithoutDemandBasedTableLoading(query);
        }
        catch (Exception ex)
        {
            var (table, visualizationState) = TableFromEvaluationResult(EvaluationResult.Null);
            return new KustoQueryResult(query, table, visualizationState, TimeSpan.Zero, ex.Message);
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
        if (!tables.Any() && query.Tokenize().Length == 1) tables.Add(query.Trim());

        return tables.Select(KustoNameEscaping.RemoveFraming).Distinct()
            .Where(t=>t.IsNotBlank()).ToArray();
    }

    public IEnumerable<ITableSource> Tables()
    {
        return _tables;
    }

    private KustoQueryContext AddDebug(IKustoConsole console,KustoSettingsProvider settings)
    {
        _settings = settings;
        _debugConsole = console;
        return this;
    }

    /// <summary>
    ///     Creates a context that has additional debug information
    /// </summary>
    /// <remarks>
    ///     Primarily used for testing and development
    /// </remarks>
    public static KustoQueryContext CreateWithDebug(IKustoConsole console,KustoSettingsProvider settings)
    {
        return new KustoQueryContext()
            .AddDebug(console,settings);
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

    public async Task<IReadOnlyCollection<T>> RunQueryToRecordSet<T>(string query)
    {
        var result = await RunQuery(query);
        if (result.Error.IsNotBlank())
            throw new InvalidOperationException($"{result.Error}");
        return result.ToRecords<T>();
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


    /// <summary>
    ///     Runs a query against the supplied records.
    /// </summary>
    /// <remarks>
    ///     The query is prefixed with "data | " so only basic queries are supported
    /// </remarks>
    public static KustoQueryResult QueryRecords<T>(
        ImmutableArray<T> rows,
        string query)
    {
        const string tableName = "data";
        var context = new KustoQueryContext();
        query = $"{tableName} | {query}";
        context.WrapDataIntoTable(tableName, rows);
        return context.RunQueryWithoutDemandBasedTableLoading(query);
    }

    /// <summary>
    ///     Runs a query against the supplied records.
    /// </summary>
    /// <remarks>
    ///     This is a convenience method intended to support programmatically generated queries.  It should be used with
    ///     caution since it's quite possible to change the shape of the data in the query in a way that would make it
    ///     impossible
    ///     to deserialize the results back into the original type.
    /// </remarks>
    public static IReadOnlyCollection<T> FilterRecords<T>(ImmutableArray<T> rows,
        string query)
    {
        var result = QueryRecords(rows, query);
        return result.ToRecords<T>();
    }

    /// <summary>
    ///     True if the context has a table with the given name
    /// </summary>
    public bool HasTable(string tableName)
    {
        tableName = KustoNameEscaping.RemoveFraming(tableName);
        return _tables.Any(t => t.Name == tableName);
    }

    public static KustoQueryContext CreateForTest()
    {
       return new KustoQueryContext()
           .AddDebug(new SystemConsole(),
           BabyKustoEngine.GetSettingsWithFullDebug()
               );
    }
}
