namespace KustoSupport;

/// <summary>
///     Interface to allow KustoQueryContext to lazily load tables as required
/// </summary>
public interface IKustoQueryContextTableLoader
{
    /// <summary>
    ///     Called with a list of table names referenced by a query
    /// </summary>
    public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames);

    Task<bool> LoadTable(KustoQueryContext context, string path, string tableName);
    Task SaveResult(KustoQueryResult result, string path);
}
