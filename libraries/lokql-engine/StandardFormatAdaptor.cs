
using KustoLoco.FileFormats;
using KustoLoco.Core;

#pragma warning disable CS8618, CS8604
public class StandardFormatAdaptor : IKustoQueryContextTableLoader
{
    private readonly IReadOnlyCollection<IFileBasedTableAccess> _loaders;
    private readonly string[] _paths;

    public StandardFormatAdaptor(params string[] paths)
    {
        _paths = paths;
        _loaders =
        [
            new CsvTableAdaptor(),
            new ParquetTableAdaptor(),
            new TextTableAdaptor(),
            new JsonArrayTableAdaptor()
        ];
    }

    public async Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames)
    {
        foreach (var path in tableNames)
        {
            if (await LoadTable(context, path, path))
                continue;
            break;
        }
    }

    public async Task SaveResult(KustoQueryResult result, string path)
    {
        var loader = GetFileLoaderForExtension(path);


        var filePaths = Path.IsPathRooted(path)
            ? [path]
            : _paths.Select(p => Path.Combine(p, path));
        foreach (var filepath in filePaths)
        {
            await loader.TrySave(filepath, result);
            //todo -here -  quick hack to ensure we only save to one place!
            break;
        }
    }

    public async Task<bool> LoadTable(KustoQueryContext context, string path, string tableName)
    {
        var alreadyPresent = context.TableNames.Contains(tableName);
        if (alreadyPresent)
            return true;

        var loader = GetFileLoaderForExtension(path);


        var filePaths = Path.IsPathRooted(path)
            ? [path]
            : _paths.Select(p => Path.Combine(p, path));
        foreach (var filepath in filePaths)
        {
            if (!Path.Exists(filepath)) break;

            var success = await loader.TryLoad(filepath, context, tableName,new NullProgressReporter());
            if (success)
                break;
        }

        return false;
    }

    public IFileBasedTableAccess GetFileLoaderForExtension(string oFile)
    {
        var ext = Path.GetExtension(oFile);
        foreach (var loader in _loaders)
        {
            if (loader.SupportedFileExtensions().Contains(ext))
                return loader;
        }

        return new NullFileLoader();
    }
}