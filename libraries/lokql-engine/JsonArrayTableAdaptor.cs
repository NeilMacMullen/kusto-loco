using KustoLoco.Core;
using KustoLoco.FileFormats;
using NLog;

namespace Lokql.Engine;

/// <summary>
///     Reads an array of objects in a json file
/// </summary>
public class JsonArrayTableAdaptor : IFileBasedTableAccess
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,
        IProgress<string> progressReporter)
    {
        var res = await new JsonObjectArraySerializer().LoadTable(path, name, progressReporter);

        context.AddTable(res.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions()
    {
        return [".json"];
    }

    public async Task TrySave(string path, KustoQueryResult result)
    {
        await new JsonObjectArraySerializer().SaveTable(path, result, new NullProgressReporter());
    }

    public TableAdaptorDescription GetDescription()
        => new("JsonObjectArray", "Array of json objects", SupportedFileExtensions());
}
