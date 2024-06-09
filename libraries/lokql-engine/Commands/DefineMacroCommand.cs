using CommandLine;

namespace Lokql.Engine.Commands;

public static class DefineMacroCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var blocks = econtext.Sequence;

        var macroBlocks = new List<string>();
        var valid =false;
        while (!blocks.Complete)
        {
            var nextBlock = blocks.Next();
            if (nextBlock == ".end")
            {
                valid = true;
                break;
            }
            macroBlocks.Add(nextBlock);
        }
        if (!valid)
        {
            exp.Warn("Macro definition incomplete - must finish with .end");
            return;
        }

        exp.AddMacro(new MacroDefinition(o.Name, o.ParameterNames, macroBlocks.ToArray()));
        
        exp.Info($"Macro '{o.Name}' defined");
        await Task.CompletedTask;

    }

    [Verb("define", aliases: ["def"],
        HelpText = "Defines a Macro")]
    internal class Options
    {
        [Value(0, HelpText = "Macro name", Required = true)]
        public string Name { get; set; } = string.Empty;
        [Value(1, HelpText = "parameter names ")]
        public IEnumerable<string> ParameterNames { get; set; } = Array.Empty<string>();
    }
}
