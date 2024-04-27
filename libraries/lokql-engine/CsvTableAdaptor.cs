
using KustoLoco.FileFormats;
using KustoLoco.Core;
using NotNullStrings;

namespace Lokql.Engine;

public class CsvTableAdaptor : IFileBasedTableAccess
{
    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter,KustoSettings settings)
    {
        var result = await CsvSerializer.Default.LoadTable(path, name,progressReporter,settings);
        context.AddTable(result.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".csv"];

    public async Task<bool> TrySave(string path, KustoQueryResult result)
    {
        return (await CsvSerializer.Default.SaveTable(path, result,new NullProgressReporter())).Error.IsBlank();
       
    }

    public TableAdaptorDescription GetDescription()
        => new("Csv", "Csv Files", SupportedFileExtensions());
}
