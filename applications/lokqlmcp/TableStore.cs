using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace LokqlMcp;

/// <summary>
///     Holds the query context populated with tables loaded from parquet files.
/// </summary>
public sealed class TableStore
{
    public TableStore(KustoSettingsProvider settings)
    {
        Settings = settings;
        Context = new KustoQueryContext();
    }

    public KustoSettingsProvider Settings { get; }
    public KustoQueryContext Context { get; }

    public async Task LoadFolderAsync(string folder)
    {
        if (!Directory.Exists(folder))
            throw new DirectoryNotFoundException($"Data folder '{folder}' does not exist");

        var serializer = new ParquetSerializer(Settings, new NullConsole());
        foreach (var path in Directory.EnumerateFiles(folder, "*.parquet").Order())
        {
            var tableName = Path.GetFileNameWithoutExtension(path);
            var result = await serializer.LoadTable(path, tableName);
            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                Console.Error.WriteLine($"Failed to load '{path}': {result.Error}");
                continue;
            }

            Context.AddTable(result.Table);
            Console.Error.WriteLine($"Loaded table '{tableName}' ({result.Table.RowCount} rows)");
        }
    }

    public IEnumerable<ITableSource> Tables() => Context.Tables();

    public ITableSource? FindTable(string name) =>
        Context.Tables().FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
}
