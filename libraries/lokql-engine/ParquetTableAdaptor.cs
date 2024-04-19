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

    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter,KustoSettings settings)
    {
        var result = await new ParquetSerializer().LoadTable(path, name,progressReporter,settings);
        context.AddTable(result.Table);
        return true;
    }

    public TableAdaptorDescription GetDescription()
        => new("Parquet", "Apache Parquet Files", SupportedFileExtensions());
}
