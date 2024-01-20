using KustoSupport;

public interface IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name);
    public IReadOnlyCollection<string> SupportedFileExtensions();

    public Task TrySave(string path, KustoQueryResult result);
}