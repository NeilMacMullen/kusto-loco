using System.Collections.Immutable;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;


namespace Lokql.Engine;

public class TextTableAdaptor : IFileBasedTableAccess
{
    private readonly KustoSettingsProvider _settings;
    private readonly IProgress<string> _progressReporter;

    public TextTableAdaptor(KustoSettingsProvider settings,IProgress<string> progressReporter)
    {
        _settings = settings;
        _progressReporter = progressReporter;
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

    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        var lines = File.ReadAllLines(path)
                        .Select(l => new { Line = l })
                        .ToImmutableArray();
        var table = TableBuilder.CreateFromImmutableData(name, lines).ToTableSource();
        context.AddTable(table);
        return Task.FromResult(true);
    }

    public TableAdaptorDescription GetDescription()
        => new("Text", "Tabular data text format", SupportedFileExtensions());
}
