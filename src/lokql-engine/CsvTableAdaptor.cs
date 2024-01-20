using CsvSupport;
using KustoSupport;

public class CsvTableAdaptor : IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        CsvLoader.Load(path, context, name);
        return Task.FromResult(true);
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".csv"];

    public Task TrySave(string path, KustoQueryResult result)
    {
        CsvLoader.WriteToCsv(path, result);
        return Task.CompletedTask;
    }
}