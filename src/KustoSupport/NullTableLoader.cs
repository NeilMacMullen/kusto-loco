namespace KustoSupport;

/// <summary>
///     Default implementation of table loader that doesn't actually load any tables
/// </summary>
internal class NullTableLoader : IKustoQueryContextTableLoader
{
    public Task LoadTables(KustoQueryContext context, IReadOnlyCollection<string> tableNames) => Task.CompletedTask;
}
