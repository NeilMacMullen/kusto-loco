
using KustoLoco.FileFormats;
using KustoLoco.Core;

public class CsvTableAdaptor : IFileBasedTableAccess
{
    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter)
    {
        var result = await new CsvSerializer().LoadTable(path, name,progressReporter);
        context.AddTable(result.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".csv"];

    public Task TrySave(string path, KustoQueryResult result)
    {
        CsvSerializer.WriteToCsv(path, result);
        return Task.CompletedTask;
    }
}