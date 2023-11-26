using System.Collections.Specialized;
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
    public static async Task<IReadOnlyCollection<TR>> SimpleQuery<T, TR>(IReadOnlyCollection<T> rows, string query)
    {
        var result = await SimpleQueryToOrderedDictionary(rows, query);
        return KustoQueryContext.DeserialiseTo<TR>(result.Results);
    }

    public static async Task<KustoQueryResult<OrderedDictionary>> SimpleQueryToOrderedDictionary<T>(
        IReadOnlyCollection<T> rows, string query)
    {
        var tableName = $"Table{Guid.NewGuid():N}";
        return await RunQuery(tableName, rows, query, true);
    }


    public static async Task<KustoQueryResult<OrderedDictionary>> RunQuery<T>(string tableName,
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

        return await context.RunQuery(query);
    }

    public static async Task<KustoQueryResult<OrderedDictionary>> RunQuery<T>(string tableName,
        IReadOnlyCollection<T> rows,
        string query)
        => await RunQuery(tableName, rows, query, true);
}