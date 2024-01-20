using KustoSupport;

public class NullFileLoader : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [];

    public Task TrySave(string path, KustoQueryResult result) => Task.CompletedTask;

    public Task<bool> TryLoad(string path, KustoQueryContext context, string name) => Task.FromResult(false);
}