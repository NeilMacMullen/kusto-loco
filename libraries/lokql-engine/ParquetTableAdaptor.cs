using KustoLoco.Core;
using KustoLoco.FileFormats;

namespace Lokql.Engine;

public class ParquetTableAdaptor : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [".parquet"];

    public async Task TrySave(string path, KustoQueryResult result)
    {
        await new ParquetSerializer() .SaveTable(path, result,new NullProgressReporter());
    }

    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter)
    {
        var result = await new ParquetSerializer().LoadTable(path, name,progressReporter);
        context.AddTable(result.Table);
        return true;
    }
}