using NLog;

namespace KustoSupport;

/// <summary>
///     Provides simplified Kusto querying
/// </summary>
public class KustoQueryHelper
{
    public static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     Performs a one-shot query against a record set
    /// </summary>
    /// <remarks>
    ///     This not very efficient but extremely convenient for testing
    /// </remarks>
    public static async Task<IReadOnlyCollection<TR>> SimpleQueryTo<T, TR>(IReadOnlyCollection<T> rows, string query)
    {
        var result = await SimpleQuery(rows, query);
        return result.DeserialiseTo<TR>();
    }

    public static async Task<KustoQueryResult> SimpleQuery<T>(
        IReadOnlyCollection<T> rows, string query)
    {
        var tableName = $"Table{Guid.NewGuid():N}";
        return await RunQuery(tableName, rows, query, true);
    }

    public static async Task<IReadOnlyCollection<R>> SimpleQueryToAndFrom<T, R>(
        IReadOnlyCollection<T> rows, string query)
    {
        var result = await SimpleQuery(rows, query);
        return result.DeserialiseTo<R>();
    }


    public static async Task<KustoQueryResult> RunQuery<T>(string tableName,
        IReadOnlyCollection<T> rows,
        string query, bool appendQueryToTableName)
    {
        var context = new KustoQueryContext();
        context.AddTableFromRecords(tableName, rows);
        //fix up query...
        if (appendQueryToTableName)
        {
            query = $@"{tableName} |
{query}
";
        }

        return await context.RunTabularQueryAsync(query);
    }


    public static async Task<KustoQueryResult> RunQuery<T>(string tableName,
        IReadOnlyCollection<T> rows,
        string query)
        => await RunQuery(tableName, rows, query, true);
}
