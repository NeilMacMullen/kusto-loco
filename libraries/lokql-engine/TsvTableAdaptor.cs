using KustoLoco.Core;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class TsvTableAdaptor : IFileBasedTableAccess
{
    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name, IProgress<string> progressReporter)
    {
        var result = await CsvSerializer.Tsv.LoadTable(path, name, progressReporter);
        context.AddTable(result.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".tsv"];

    public async Task TrySave(string path, KustoQueryResult result)
    {
        await CsvSerializer.Tsv.SaveTable(path, result, new NullProgressReporter());

    }
}