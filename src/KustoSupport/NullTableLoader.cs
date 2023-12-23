namespace KustoSupport;

/// <summary>
///     Default implementation of table loader that doesn't actually load any tables
/// </summary>
internal class NullTableLoader : IKustoQueryContextTableLoader
{
    public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames)
        => Task.CompletedTask;
}
