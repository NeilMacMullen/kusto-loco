using KustoLoco.Core;

public interface IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter);
    public IReadOnlyCollection<string> SupportedFileExtensions();

    public Task TrySave(string path, KustoQueryResult result);
}