using System.Text;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine;

public class Explorer
{
    private readonly IKustoConsole _console;
    private readonly InteractiveTableExplorer _explorer;

    private Explorer(string dataFolder, IEnumerable<string> args)
    {
        _console = new SystemConsole();
        var settings = new KustoSettingsProvider();
       
        settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, dataFolder);
        foreach (var (i, a) in args.Index()) settings.Set($"arg{i}", a);

        var processor = CommandProcessorProvider.GetCommandProcessor();
        var renderer = new SixelRenderingSurface(settings);
        _explorer = new InteractiveTableExplorer(_console, settings, processor, renderer, []);
      
    }

    public static async Task<Explorer> Create(string dataFolder, IEnumerable<string> args,
        IEnumerable<string> dataFiles,
        IEnumerable<string> commands,
        IEnumerable<string> scripts)
    {
        var r = new Explorer(dataFolder, args);
        foreach (var datafile in dataFiles)
        {
            await r.RunInput($".load \"{datafile}\"");
        }
        foreach (var block in commands)
            await r.RunInput(block);
        foreach (var f in scripts)
            try
            {
                var block = await File.ReadAllTextAsync(f);
                await r.RunInput(block);
            }
            catch (IOException)
            {
                r.Error("Unable to load script '{f}'");
            }
        return r;
    }

    public void Close() => _console.RestoreColors();

    public async Task RunInput(string block) => await _explorer.RunInput(block);

    public async Task RunInteractive()
    {
        _explorer.Warn("Use '.help' to list commands");

        var isContinuation = false;
        var query = new StringBuilder();
        while (true)
        {
            _console.ForegroundColor = ConsoleColor.Blue;
            var prompt = isContinuation ? "   > " : "KQL> ";
            _console.Write(prompt);
            var queryPart = _console.ReadLine().Trim();
            var isSlashContinuation = queryPart.EndsWith(@"\");
            isContinuation = isSlashContinuation
                             || queryPart.EndsWith("|");
            if (isSlashContinuation)
                queryPart = queryPart.Substring(0, queryPart.Length - 1);
            query.Append(queryPart);
            if (!isContinuation)
            {
                await _explorer.RunInput(query.ToString());
                query.Clear();
            }
        }
    }

    public void Error(string err)
    {
       _console.Error(err);
    }
}
