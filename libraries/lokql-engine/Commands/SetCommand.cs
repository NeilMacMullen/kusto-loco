using CommandLine;

namespace Lokql.Engine.Commands;

public static class SetCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        exp.Settings.Set(o.Name, o.Value);
        exp.Info($"Set {o.Name} to {o.Value}");
        return Task.CompletedTask;
    }

    [Verb("set", HelpText = "sets a setting value")]
    internal class Options
    {
        [Value(0, HelpText = "setting name", Required = true)]
        public string Name { get; set; } = string.Empty;

        [Value(1, HelpText = "Value of setting (omit to remove)")]
        public string Value { get; set; } = string.Empty;
    }
}
