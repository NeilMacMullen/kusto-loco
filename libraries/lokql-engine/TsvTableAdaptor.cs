using KustoLoco.Core;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace Lokql.Engine;

public class TsvTableAdaptor : IFileBasedTableAccess
{
    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name, IProgress<string> progressReporter,KustoSettings settings)
    {
        var result = await CsvSerializer.Tsv.LoadTable(path, name, progressReporter,settings);
        context.AddTable(result.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".tsv"];

    public async Task<bool> TrySave(string path, KustoQueryResult result)
    {
        return (await CsvSerializer.Tsv.SaveTable(path, result, new NullProgressReporter())).Error.IsBlank();

    }

    public TableAdaptorDescription GetDescription()
        => new("Tsv", "Tab-separated data", SupportedFileExtensions());
}
