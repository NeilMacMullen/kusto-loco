using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using NotNullStrings;

namespace Lokql.Engine;

/// <summary>
///     Table IO adaptor that can load and save tables for in a variety of standard formats
/// </summary>
/// <remarks>
///     This class is used by the interactive shell to choose the appropriate adaptor based on
///     the file extension.  For example, if the user tries to load a file called "mydata.csv",
///     the CsvTableAdaptor will be used to load the file.
///     The adaptor will try to load the file from a list of paths, in order
///     unless the path is fully qualified.
/// </remarks>
public class StandardFormatAdaptor : ITableAdaptor
{
    private readonly IKustoConsole _console;
    private readonly IReadOnlyCollection<IFileBasedTableAccess> _loaders;
    private readonly KustoSettingsProvider _settings;

    public StandardFormatAdaptor(KustoSettingsProvider settings, IKustoConsole console)
    {
        _settings = settings;
        _console = console;
        _loaders =
        [
            new CsvTableAdaptor(settings, console),
            new TsvTableAdaptor(settings, console),
            new ParquetTableAdaptor(settings, console),
            new TextTableAdaptor(settings, console),
            new JsonArrayTableAdaptor(settings, console),
            new ExcelTableAdaptor(settings, console)
        ];
        _settings.Register(Settings.KustoDataPath);
    }

    private IReadOnlyCollection<string> Paths => _settings.GetPathList(Settings.KustoDataPath);

    public async Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames)
    {
        foreach (var path in tableNames)
        {
            if (await LoadTable(context, path, path))
                continue;
            break;
        }
    }

    public async Task<bool> SaveResult(KustoQueryResult result, string path)
    {
        if (result.RowCount == 0)
        {
            _console.Warn("No rows to save");
            return false;
        }

        var loader = GetFileLoaderForExtension(path);


        var filePaths = Path.IsPathRooted(path)
            ? [path]
            : Paths.Select(p => Path.Combine(p, path));
        foreach (var filepath in filePaths)
        {
            var success = await loader.TrySave(filepath, result);
            //todo -here -  quick hack to ensure we only save to one place!
            if (success)
            {
                _console.Info($"Saved {result.RowCount} rows x {result.ColumnCount} columns to {filepath}");
                return success;
            }
        }

        _console.Info($"Unable to save result to {path}");
        return false;
    }

    public IEnumerable<TableAdaptorDescription> GetSupportedAdaptors()
    {
        return _loaders.Select(l => l.GetDescription()).ToArray();
    }

    public void SetDataPaths(string path)
    {
        _settings.Set(Settings.KustoDataPath.Name, path);
    }

    public async Task<bool> LoadTable(KustoQueryContext context, string path, string tableName)
    {
        var alreadyPresent = context.HasTable(tableName);
        if (alreadyPresent)
            return true;

        var loader = GetFileLoaderForExtension(path);


        var filePaths = Path.IsPathRooted(path)
            ? [path]
            : Paths.Select(p => Path.Combine(p, path));
        foreach (var filepath in filePaths)
        {
            if (!Path.Exists(filepath))
                break;

            var result = await loader.TryLoad(filepath, tableName);
            if (result.Error.IsBlank())
            {
                context.AddTable(result.Table);
                _console.Info($"Loaded table '{tableName}' from {filepath}");
                return true;
            }

            _console.Warn($"Unable to load table '{tableName}' from {filepath}");
            _console.Warn($"Error:{result.Error}");
        }

        _console.Warn($"Unable to load table '{tableName}' from {path}");
        return false;
    }

    public IFileBasedTableAccess GetFileLoaderForExtension(string oFile)
    {
        var ext = Path.GetExtension(oFile);
        foreach (var loader in _loaders)
            if (loader.SupportedFileExtensions().Contains(ext))
                return loader;

        return new NullFileLoader();
    }

    public static class Settings
    {
        public static readonly KustoSettingDefinition KustoDataPath = new("kusto.datapath",
            "Search path for kusto data files", @"C:\kusto", nameof(String));
    }
}
