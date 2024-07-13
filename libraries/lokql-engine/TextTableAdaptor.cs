using System.Collections.Immutable;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;


namespace Lokql.Engine;

public class TextTableAdaptor : IFileBasedTableAccess
{
    private readonly KustoSettingsProvider _settings;
    private readonly IKustoConsole _console;

    public TextTableAdaptor(KustoSettingsProvider settings,IKustoConsole console)
    {
        _settings = settings;
        _console = console;
    }
    public IReadOnlyCollection<string> SupportedFileExtensions() => [".txt"];

    public Task<bool> TrySave(string path, KustoQueryResult result)
    {
        try
        {
            var text = KustoFormatter.Tabulate(result);
            File.WriteAllText(path, text);
            return Task.FromResult(true);
        }
        catch 
        {
            return Task.FromResult(false);
        }
    }

    public Task<TableLoadResult> TryLoad(string path, string name)
    {
        var lines = File.ReadAllLines(path)
                        .Select(l => new { Line = l })
                        .ToImmutableArray();
        var table = TableBuilder.CreateFromImmutableData(name, lines).ToTableSource();
       
        return Task.FromResult(TableLoadResult.Success(table));
    }

    public TableAdaptorDescription GetDescription()
        => new("Text", "Tabular data text format", SupportedFileExtensions());
}
