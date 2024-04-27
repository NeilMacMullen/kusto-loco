using KustoLoco.Core;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace Lokql.Engine;

public class ParquetTableAdaptor : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [".parquet"];

    public async Task<bool> TrySave(string path, KustoQueryResult result)
    {
       return (await new ParquetSerializer() .SaveTable(path, result,new NullProgressReporter())).Error.IsNotBlank();
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
