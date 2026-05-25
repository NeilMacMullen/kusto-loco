using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using System.Collections.Immutable;
using static KustoLoco.Core.KustoFormatter;


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
            var skipHeader = _settings.GetBool("txt.skipheader");
            var prefs = new DisplayPreferences(int.MaxValue, 0, int.MaxValue, skipHeader);
            var text = KustoFormatter.Tabulate(result,prefs);
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
