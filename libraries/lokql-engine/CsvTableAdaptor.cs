
using KustoLoco.FileFormats;
using KustoLoco.Core;
namespace Lokql.Engine;

public class CsvTableAdaptor : IFileBasedTableAccess
{
    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter)
    {
        var result = await CsvSerializer.Default.LoadTable(path, name,progressReporter);
        context.AddTable(result.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".csv"];

    public async Task TrySave(string path, KustoQueryResult result)
    {
        await CsvSerializer.Default.SaveTable(path, result,new NullProgressReporter());
       
    }

    public TableAdaptorDescription GetDescription()
        => new("Csv", "Csv Files", SupportedFileExtensions());
}
