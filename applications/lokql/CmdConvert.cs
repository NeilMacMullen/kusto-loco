using CommandLine;
using NLog;

/// <summary>
///     Interactive data explorer
/// </summary>
internal class CmdConvert
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var explorer = await Explorer.Create(options.Data, options.Args, options.Load,
            options.Commands, options.Scripts);
        await explorer.RunInput($".load \"{options.In}\"");
        await explorer.RunInput($".save \"{options.Out}\"");
        explorer.Close();
    }

    [Verb("convert",  HelpText = "Converts a data file into another format ")]
    public class Options : CommonOptions
    {
        [Option('i', Required = true, HelpText = "Input file")]
        public string In { get; set; } = string.Empty;
        [Option('o', Required = true, HelpText = "Output file")]
        public string Out { get; set; } = string.Empty;
        [Option('f', Required = false, HelpText = "Format")]
        public string Format { get; set; } = string.Empty;
    }
}
