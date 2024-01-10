using System.Collections.Specialized;
using System.Text.Json;
using CsvSupport;
using KustoSupport;
using ParquetSupport;

#pragma warning disable CS8618, CS8604
public class OurTableLoader : IKustoQueryContextTableLoader
{
    private readonly IReadOnlyCollection<IFileBasedTableAccess> _loaders;
    private readonly string[] _paths;

    public OurTableLoader(params string[] paths)
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
        var alreadyPresent = context.TableNames.Contains(path);
        if (alreadyPresent)
            return true;

        var loader = GetFileLoaderForExtension(path);


        var filePaths = Path.IsPathRooted(path)
                            ? [path]
                            : _paths.Select(p => Path.Combine(p, path));
        foreach (var filepath in filePaths)
        {
            if (!Path.Exists(filepath)) break;

            var success = await loader.TryLoad(filepath, context, tableName);
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

public interface IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name);
    public IReadOnlyCollection<string> SupportedFileExtensions();

    public Task TrySave(string path, KustoQueryResult result);
}

public class CsvTableAdaptor : IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        CsvLoader.Load(path, context, name);
        return Task.FromResult(true);
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".csv"];

    public Task TrySave(string path, KustoQueryResult result)
    {
        CsvLoader.WriteToCsv(path, result);
        return Task.CompletedTask;
    }
}

/// <summary>
///     Reads an array of objects in a json file
/// </summary>
public class JsonArrayTableAdaptor : IFileBasedTableAccess
{
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        var text = File.ReadAllText(path);
        var dict = JsonSerializer.Deserialize<OrderedDictionary[]>(text);
        var table = TableBuilder
            .FromOrderedDictionarySet(name,
                                      dict);
        context.AddTable(table);
        return Task.FromResult(true);
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".json"];

    public Task TrySave(string path, KustoQueryResult result)
    {
        var json = result.ToJsonString();
        File.WriteAllText(path, json);
        return Task.CompletedTask;
    }
}

public class TextTableAdaptor : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [".txt"];

    public Task TrySave(string path, KustoQueryResult result)
    {
        var text = KustoFormatter.Tabulate(result);
        File.WriteAllText(path, text);
        return Task.CompletedTask;
    }

    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        var lines = File.ReadAllLines(path)
                        .Select(l => new { Line = l })
                        .ToArray();
        var table = TableBuilder.CreateFromRows(name, lines).ToTableSource();
        context.AddTable(table);
        return Task.FromResult(true);
    }
}


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


public class NullFileLoader : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [];

    public Task TrySave(string path, KustoQueryResult result) => Task.CompletedTask;

    public Task<bool> TryLoad(string path, KustoQueryContext context, string name) => Task.FromResult(false);
}
