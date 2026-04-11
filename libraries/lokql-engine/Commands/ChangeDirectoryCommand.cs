using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Change the current data directory (kusto.datapath)
/// </summary>
public static class ChangeDirectoryCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var settings = context.Settings;
        settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, o.Path);
        console.Info($"Data path set to {o.Path}");
        return Task.CompletedTask;
    }

    [Verb("cd", HelpText = @"sets the data directory path (kusto.datapath)
The data directory is used as the search path for loading files.
Examples:
  .cd C:\mydata
  .cd D:\work\project1")]
    internal class Options
    {
        [Value(0, HelpText = "directory path", Required = true)]
        [FileOptions(FoldersOnly = true)]
        public string Path { get; set; } = string.Empty;
    }
}
