namespace KustoSupport;

/// <summary>
///     Interface to allow KustoQueryContext to lazily load tables as required
/// </summary>
/// <remarks>
///     It's important to understand that it is the _tableloader_ that is responsible
///     for correctly pipelining the addition of tables since the context is not
///     inherently thread-safe.
/// </remarks>
public interface IKustoQueryContextTableLoader
{
    /// <summary>
    ///     Called with a list of table names referenced by a query
    /// </summary>
    public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames);

    Task<bool> LoadTable(KustoQueryContext context, string path, string tableName);
    Task SaveResult(KustoQueryResult result, string path);
}