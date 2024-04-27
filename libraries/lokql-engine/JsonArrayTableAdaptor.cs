using KustoLoco.Core;
using KustoLoco.FileFormats;
using NLog;
using NotNullStrings;

namespace Lokql.Engine;

/// <summary>
///     Reads an array of objects in a json file
/// </summary>
public class JsonArrayTableAdaptor : IFileBasedTableAccess
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,
        IProgress<string> progressReporter,KustoSettings settings)
    {
        var res = await new JsonObjectArraySerializer().LoadTable(path, name, progressReporter,settings);

        context.AddTable(res.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions()
    {
        return [".json"];
    }

    public async Task<bool> TrySave(string path, KustoQueryResult result)
    {
        return (await new JsonObjectArraySerializer().SaveTable(path, result, new NullProgressReporter())).Error.IsBlank();
    }

    public TableAdaptorDescription GetDescription()
        => new("JsonObjectArray", "Array of json objects", SupportedFileExtensions());
}
