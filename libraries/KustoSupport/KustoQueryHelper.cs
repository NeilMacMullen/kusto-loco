using System.Collections.Immutable;
using NLog;

namespace KustoLoco.Core;


/// <summary>
///     Provides simplified Kusto querying
/// </summary>
public class KustoQueryHelper
{
    public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     Run a very basic query against a set of rows
    /// </summary>
    /// <remarks></remarks>
    public static async Task<KustoQueryResult> SimpleQuery<T>(
        ImmutableArray<T> rows, string query)
    {
        var tableName = $"Table{Guid.NewGuid():N}";
        return await RunQueryWithTableNamePiped(tableName, rows, query);
    }

    /// <summary>
    ///     Runs a basic query from a set of rows to a different projection
    /// </summary>
    /// <remarks> Will just throw/return empty if the result is not as expected</remarks>
    public static async Task<IReadOnlyCollection<R>> SimpleQueryToAndFrom<T, R>(
        ImmutableArray<T> rows, string query)
    {
        var result = await SimpleQuery(rows, query);
        return result.DeserialiseTo<R>();
    }

    /// <summary>
    ///     Runs a query against the supplied rows.
    /// </summary>
    /// <remarks>
    ///     The
    /// </remarks>
    public static async Task<KustoQueryResult> RunQuery<T>(string tableName,
        ImmutableArray<T> rows,
        string query)
    {
        Logger.Info("Create context");
        var context = new KustoQueryContext();
        Logger.Info("Adding table from records");
        context.AddTableFromRecords(tableName, rows);
        Logger.Info("Running...");

        return await context.RunTabularQueryAsync(query);
    }

    /// <summary>
    ///     Runs a query against the supplied records
    /// </summary>
    /// <remarks>
    ///     The query should contain the name of the query, e.g. "data | count"
    /// </remarks>
    public static Task<KustoQueryResult> RunQueryWithTableNamePiped<T>(string tableName,
        ImmutableArray<T> rows,
        string query)
    {
        query = PipeTableName(tableName, query);
        return RunQuery(tableName, rows, query);
    }

    public static string PipeTableName(string tableName, string query) => $"{tableName} | {query}";
}
