using KustoSupport;
using ParquetSupport;

public class ParquetTableAdaptor : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [".parquet"];

    public async Task TrySave(string path, KustoQueryResult result)
    {
        await ParquetFileOps.Save(path, result);
    }

    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        var table = await ParquetFileOps.LoadFromFile(path, name);
        context.AddTable(table);
        return true;
    }
}