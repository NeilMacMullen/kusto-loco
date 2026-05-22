using CommandLine;
using NLog;

/// <summary>
///     Interactive data explorer
/// </summary>
internal class CmdRun
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var explorer = new Explorer(options.Data, options.Args);
        foreach (var block in options.Commands)
            await explorer.RunInput(block);
        foreach (var f in options.Files)
            try
            {
                var block = await File.ReadAllTextAsync(f);
                await explorer.RunInput(block);
            }
            catch (IOException)
            {
                explorer.Error("Unable to load file '{f}'");
            }

        explorer.Close();
    }

    [Verb("run", true, HelpText = "runs a lokqldx workspace in its entirety")]
    public class Options
    {
        [Option(HelpText = "Default folder to load/save data/results to")]
        public string Data { get; set; } = string.Empty;

        [Option('f', HelpText = "Runs a script or sequence of scripts")]
        public IEnumerable<string> Files { get; set; } = [];

        [Option('c', HelpText = "Runs a command or sequence of commands")]
        public IEnumerable<string> Commands { get; set; } = [];

        [Option("args", HelpText = "Passes arguments to the script as arg0, arg1 etc")]
        public IEnumerable<string> Args { get; set; } = [];
    }
}
