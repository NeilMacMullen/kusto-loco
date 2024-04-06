using System.Collections.Specialized;
using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.FileFormats;
using NLog;

/// <summary>
///     Reads an array of objects in a json file
/// </summary>
public class JsonArrayTableAdaptor : IFileBasedTableAccess
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public async Task<bool> TryLoad(string path, KustoQueryContext context, string name,IProgress<string> progressReporter)
    {

        var res = await new JsonObjectArraySerializer().LoadTable(path, name, progressReporter);
      
        context.AddTable(res.Table);
        return true;
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".json"];

    public async Task TrySave(string path, KustoQueryResult result)
    {
        await new JsonObjectArraySerializer().SaveTable(path, result,new NullProgressReporter());
    }
}