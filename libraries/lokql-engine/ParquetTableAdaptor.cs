using KustoLoco.Core;
using KustoLoco.FileFormats;



public class ParquetTableAdaptor : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [".parquet"];

    public async Task TrySave(string path, KustoQueryResult result)
    {
        await ParquetFileOps.Save(path, result);
    }

    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter)
    {
        var result = await new ParquetFileOps().LoadTable(path, name,progressReporter);
        context.AddTable(result.Table);
        return true;
    }
}