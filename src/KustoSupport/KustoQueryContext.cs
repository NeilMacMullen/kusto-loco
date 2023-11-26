using System.Collections.Specialized;
using System.Diagnostics;
using System.Text.Json;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using Extensions;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using NLog;

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.

namespace KustoSupport;

/// <summary>
///     Provides a context for complex queries or persistent tables
/// </summary>
public class KustoQueryContext
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly BabyKustoEngine _engine = new();
    private readonly List<InMemoryKustoTable> _tables = new();

    private IKustoQueryContextTableLoader _lazyTableLoader = new NullTableLoader();


    public IEnumerable<string> TableNames => Tables().Select(t => t.Name);

    public void AddTable(InMemoryKustoTable table)
    {
        if (_tables.Any(t => t.Name == table.Name))
            throw new ArgumentException($"Context already contains a table named '{table.Name}'");
        _tables.Add(table);
        _engine.AddGlobalTable(table);
    }

    public InMemoryKustoTable GetTable(string name)
    {
        return _tables.Single(t => UnescapeTableName(t.Name) == name);
    }

    public void AddTableFromRecords<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var table = InMemoryKustoTable.CreateFromRows(tableName, records);
        AddTable(table);
    }

    public int BenchmarkQuery(string query)
    {
        var res = _engine.Evaluate(query,
            dumpIRTree: false
        );
        return Materialise(res as TabularResult);
    }

    public IReadOnlyCollection<OrderedDictionary> RunTabularQueryToDictionarySet(string query)
    {
        //handling for "special" commands
        if (query.Trim() == ".tables")
        {
            return _tables.Select(table => new OrderedDictionary
                    {
                        ["Name"] = table.Name,
                        ["Length"] = table.Length,
                    }
                )
                .ToArray();
        }

        var result =
            _engine.Evaluate(query,
                dumpIRTree: false
            );
        return GetDictionarySet(result as TabularResult);
    }

    public void AddLazyTableLoader(IKustoQueryContextTableLoader loader) => _lazyTableLoader = loader;

    public async Task<KustoQueryResult<OrderedDictionary>> RunQuery(string query)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            var requiredTables = GetTableList(query);
            var unloadedTables = requiredTables.Except(_tables.Select(t => t.Name)).ToArray();
            if (unloadedTables.Any())
                await _lazyTableLoader.LoadTables(this, unloadedTables);

            var results = RunTabularQueryToDictionarySet(query);
            return new KustoQueryResult<OrderedDictionary>(query, results,
                (int)watch.ElapsedMilliseconds,
                string.Empty);
        }
        catch (Exception ex)
        {
            return new KustoQueryResult<OrderedDictionary>(query, Array.Empty<OrderedDictionary>(), 0, ex.Message);
        }
    }

    public async Task<KustoQueryResult<T>> RunTabularQueryToRecordSet<T>(string query)
    {
        var d = await RunQuery(query);
        var rows = DeserialiseTo<T>(d.Results);
        return new KustoQueryResult<T>(d.Query, rows, d.QueryDuration, d.Error);
    }

    public static IReadOnlyCollection<OrderedDictionary> GetDictionarySet(TabularResult result)
    {
        var items = new List<OrderedDictionary>();
        if (result is TabularResult tabularResult)
        {
            var table = tabularResult.Value;
            foreach (var chunk in table.GetData())
            {
                for (var i = 0; i < chunk.RowCount; i++)
                {
                    var d = new OrderedDictionary();
                    for (var c = 0; c < chunk.Columns.Length; c++)
                    {
                        var v = chunk.Columns[c].RawData.GetValue(i);
                        d[table.Type.Columns[c].Name] = v;
                    }

                    items.Add(d);
                }
            }
        }

        return items;
    }

    public static int Materialise(TabularResult result)
    {
        var count = 0;
        if (result is TabularResult tabularResult)
        {
            var table = tabularResult.Value;
            foreach (var chunk in table.GetData())
            {
                for (var i = 0; i < chunk.RowCount; i++)
                {
                    for (var c = 0; c < chunk.Columns.Length; c++)
                    {
                        var v = chunk.Columns[c].RawData.GetValue(i);
                        count++;
                    }
                }
            }
        }

        return count;
    }


    /// <summary>
    ///     Deserialises a Dictionary-based result to objects
    /// </summary>
    public static IReadOnlyCollection<T> DeserialiseTo<T>(IReadOnlyCollection<OrderedDictionary> results)
    {
        //this is horrible but I don't have time to research how to do it ourselves and the bottom line
        //is that we are expecting results sets to be small to running through the JsonSerializer is
        //"good enough" for now...

        T ToType(OrderedDictionary d)
        {
            var json = JsonSerializer.Serialize(d);
            return JsonSerializer.Deserialize<T>(json);
        }

        return results.Select(ToType).ToArray();
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

    public IEnumerable<InMemoryKustoTable> Tables() => _tables;

    public static string UnescapeTableName(string tableName)
    {
        if ((tableName.StartsWith("['") && tableName.EndsWith("']")) ||
            (tableName.StartsWith("[\"") && tableName.EndsWith("\"]"))
           )
            return tableName.Substring(2, tableName.Length - 4);
        return tableName;
    }

    public static string EnsureEscapedTableName(string tableName) => $"['{UnescapeTableName(tableName)}']";
}