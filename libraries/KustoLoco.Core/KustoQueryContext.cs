using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Evaluation.BuiltIns;
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

    private bool _fullDebug;

    private IKustoQueryContextTableLoader _lazyTableLoader = new NullTableLoader();

    private List<ITableSource> _tables = [];

    public IEnumerable<string> TableNames => Tables().Select(t => t.Name);

    //TODO - ugh - don't like exposing this in this way
    public void AddFunctions(Dictionary<FunctionSymbol, ScalarFunctionInfo> additionalFunctions)
    {
        _additionalFunctions = additionalFunctions;
    }


    public void AddTable(TableBuilder builder)
    {
        AddTable(builder.ToTableSource());
    }


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
        RemoveTable(oldName);
        return this;
    }

    public void AddTableFromVolatileData<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var table = TableBuilder.CreateFromVolatileData(tableName, records);
        AddTable(table);
    }

    public KustoQueryContext AddTableFromImmutableData<T>(string tableName, ImmutableArray<T> records)
    {
        var table = TableBuilder.CreateFromImmutableData(tableName, records);
        AddTable(table);
        return this;
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

            var tr = TableBuilder.CreateFromImmutableData("tables", rows)
                .ToTableSource() as InMemoryTableSource;

            return new KustoQueryResult(query, tr!, VisualizationState.Empty, 0, string.Empty);
        }
        else
        {
            var rows = _tables.Select(table => new { Table = table.Name, Columns = table.Type.Columns.Count })
                    .ToImmutableArray()
                ;

            var tr = TableBuilder.CreateFromImmutableData("tables", rows)
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
        if (!tables.Any() && query.Tokenize().Length == 1) tables.Add(query.Trim());

        return tables.Select(KustoNameEscaping.RemoveFraming).Distinct().ToArray();
    }

    public IEnumerable<ITableSource> Tables() => _tables;

    private KustoQueryContext AddDebug()
    {
        _fullDebug = true;
        return this;
    }

    /// <summary>
    ///     Creates a context that has addition debug information
    /// </summary>
    /// <remarks>
    ///     Primarily used for testing and development
    /// </remarks>
    public static KustoQueryContext CreateWithDebug() => new KustoQueryContext().AddDebug();


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
        context.AddTableFromImmutableData(tableName, rows);
        return context.RunTabularQueryWithoutDemandBasedTableLoading(query);
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

    public bool HasTable(string tableName)
    {
        tableName = KustoNameEscaping.RemoveFraming(tableName);
       return  _tables.Any(t => t.Name == tableName);
    }
}