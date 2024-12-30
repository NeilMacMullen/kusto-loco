using CommandLine;
using CsvHelper.Configuration.Attributes;

namespace Lokql.Engine.Commands;

/// <summary>
/// Set a value in the settings provider
/// </summary>
public static class SetCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        exp.Settings.Set(o.Name, o.Value);
        exp.Info($"Set {o.Name} to {o.Value}");
        return Task.CompletedTask;
    }

    [Verb("set", HelpText = @"sets a value in the settings table
Settings are used to control the behavior of the engine and can be used as internal variables
Use the .settings command to show the current value of all settings
Use the .knownsettings command to show the list of 'engine' settings
Examples:
  .set csv.separator ;   #Tell csv loader to use semi-colon separated columns
  .set col LongName      #Allow queries to reference 'LongName' using $col")]
    internal class Options
    {
        [Value(0, HelpText = "setting name", Required = true)]
        public string Name { get; set; } = string.Empty;

        [Value(1, HelpText = "Value of setting (omit to remove)")]
        public string Value { get; set; } = string.Empty;
    }
}
